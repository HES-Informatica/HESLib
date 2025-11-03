using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Extensions.BR;
using HES;
using HES.Documents;
using HES.Documents.Contents.Composition;
using HES.Files;

namespace Extensions
{
    public static partial class Util
    {
        #region Internal Fields




        internal const string FormatoMoeda = "#,0.00##";
        internal const string FormatoNumero = "#,0.####";
        internal const string FormatoNumeroNF = @"000\.000\.000";



        internal const float PointFactor = 72F / 25.4F;

        internal const string TextoConsulta = "Consulta de autenticidade no portal nacional da NF-e www.nfe.fazenda.gov.br/portal ou no site da Sefaz Autorizadora";


        #endregion Internal Fields

        #region Internal Methods

        internal static Estilo CriarEstilo(float tFonteCampoCabecalho = 6, float tFonteCampoConteudo = 10) => new Estilo(DANFE.FonteRegular, DANFE.FonteNegrito, DANFE.FonteItalico, tFonteCampoCabecalho, tFonteCampoConteudo);

        internal static string Formatar(this double number, string formato = FormatoMoeda) => number.ToString(formato, Cultura);

        internal static string Formatar(this int number, string formato = FormatoMoeda) => number.ToString(formato, Cultura);

        internal static string Formatar(this int? number, string formato = FormatoMoeda) => number.HasValue ? number.Value.Formatar(formato) : string.Empty;

        internal static string Formatar(this double? number, string formato = FormatoMoeda) => number.HasValue ? number.Value.Formatar(formato) : string.Empty;

        internal static string Formatar(this DateTime? dateTime) => dateTime.HasValue ? dateTime.Value.ToString("dd/MM/yyyy") : string.Empty;

        internal static string Formatar(this TimeSpan? timeSpan) => timeSpan.HasValue ? timeSpan.Value.ToString() : string.Empty;


  

        internal static string FormatarMoeda(this double? number) => number.HasValue ? number.Value.ToString("C", Cultura) : string.Empty;

        internal static Assembly GetAssembly() => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        internal static AssemblyName GetAssemblyName() => GetAssembly()?.GetName();

        internal static string GetCompanyName()
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(GetAssembly().Location);
            var c = versionInfo?.CompanyName;
            return c.IfBlank("H&S Technologies");
        }

        #endregion Internal Methods

        #region Public Fields

        public const float A4Altura = 297;

        public const float A4Largura = 210;

        /// <summary>
        /// Altura do campo em milímetros.
        /// </summary>
        public const float CampoAltura = 6.75F;

        /// <summary>
        /// Cultura pt-BR
        /// </summary>
        public static readonly CultureInfo Cultura = new CultureInfo(1046);

        #endregion Public Fields

        #region Public Constructors

        static Util()
        {
            Cultura.NumberFormat.CurrencyPositivePattern = 2;
            Cultura.NumberFormat.CurrencyNegativePattern = 9;
        }

        #endregion Public Constructors

        #region Public Methods

        public static StringBuilder AppendChaveValor(this StringBuilder sb, string chave, string valor)
        {
            if (sb.Length > 0) sb.Append(' ');
            return sb.Append(chave).Append(": ").Append(valor);
        }

        public static RectangleF CutBottom(this RectangleF r, float height) => new RectangleF(r.X, r.Y, r.Width, r.Height - height);

        public static RectangleF CutLeft(this RectangleF r, float width) => new RectangleF(r.X + width, r.Y, r.Width - width, r.Height);

        public static RectangleF CutTop(this RectangleF r, float height) => new RectangleF(r.X, r.Y + height, r.Width, r.Height - height);

        public static RectangleF InflatedRetangle(this RectangleF rect, float top, float button, float horizontal) => new RectangleF(rect.X + horizontal, rect.Y + top, rect.Width - 2 * horizontal, rect.Height - top - button);

        public static RectangleF InflatedRetangle(this RectangleF rect, float value) => rect.InflatedRetangle(value, value, value);

        public static PdfFile MergePDF(params PdfFile[] files)
        {
            files = files ?? Array.Empty<PdfFile>();
            if (files.Any())
            {
                var outputFile = new PdfFile();

                foreach (var inputFile in files)
                {
                    foreach (var item in inputFile.Document.Pages)
                    {
                        var p = item.Clone(outputFile.Document) as Page;
                        outputFile.Document.Pages.Add(p);
                    }
                }

                return outputFile;
            }
            return null;
        }

        public static FileInfo MergePDF(this DirectoryInfo inputFiles, string outputFilePath) => MergePDF(outputFilePath, SerializationModeEnum.Incremental, inputFiles.GetFiles("*.pdf").Select(x => x.FullName).OrderBy(x => x.FileNameAsTitle()).ToArray());

        public static FileInfo MergePDF(this DirectoryInfo inputFiles, SerializationModeEnum SerializationMode, string outputFilePath) => MergePDF(outputFilePath, SerializationMode, inputFiles.GetFiles("*.pdf").Select(x => x.FullName).OrderBy(x => x.FileNameAsTitle()).ToArray());

        public static FileInfo MergePDF(string outputFilePath, params string[] inputFiles) => MergePDF(outputFilePath, SerializationModeEnum.Incremental, inputFiles);

        public static FileInfo MergePDF(string outputFilePath, SerializationModeEnum SerializationMode, params string[] inputFiles)
        {
            if (outputFilePath.IsValid() && outputFilePath.IsFilePath())
            {
                inputFiles = inputFiles ?? Array.Empty<string>();
                var files = inputFiles.Where(x => x.IsFilePath() && File.Exists(x)).Select(x => new PdfFile(x)).ToArray();
                if (files.Any())
                {
                    using (var f = MergePDF(files))
                    {
                        f?.Save(outputFilePath, SerializationMode);
                    }
                }

                return new FileInfo(outputFilePath);
            }
            else throw new ArgumentException("outputFilePath não é um caminho de arquivo válido", nameof(outputFilePath));
        }

        /// <summary>
        /// Verifica se uma string contém outra string no formato chave: valor.
        /// </summary>
        public static bool StringContemChaveValor(this string str, string chave, string valor)
        {
            if (Util.IsNotValid(chave)) throw new ArgumentException(nameof(chave));
            if (Util.IsNotValid(str)) return false;

            return Regex.IsMatch(str, $@"({chave}):?\s*{valor}", RegexOptions.IgnoreCase);
        }



        public static byte[] ToBytes(this PdfFile file, HES.Files.SerializationModeEnum SerializationMode)
        {
            byte[] bt = Array.Empty<byte>();
            using (var m = new MemoryStream())
            {
                using (var s = new HES.Bytes.Stream(m))
                {
                    file.Save(s, SerializationMode);
                    bt = m.ToBytes();

                }
            }
            return bt;
        }

        /// <summary>
        /// Converts Point to Millimeters
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float ToMm(this float point) => point / PointFactor;

        /// <summary>
        /// Converts Point to Millimeters
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static SizeF ToMm(this SizeF s) => new SizeF(s.Width.ToMm(), s.Height.ToMm());

        /// <summary>
        /// Converts Point to Millimeters
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static double ToMm(this double point) => point / PointFactor;

        public static XAlignmentEnum ToPdfClownAlignment(this AlinhamentoHorizontal ah)
        {
            switch (ah)
            {
                case AlinhamentoHorizontal.Centro:
                    return XAlignmentEnum.Center;

                case AlinhamentoHorizontal.Direita:
                    return XAlignmentEnum.Right;

                case AlinhamentoHorizontal.Esquerda:
                default:
                    return XAlignmentEnum.Left;
            }

            throw new InvalidOperationException();
        }

        public static YAlignmentEnum ToPdfClownAlignment(this AlinhamentoVertical av)
        {
            switch (av)
            {
                case AlinhamentoVertical.Topo:
                    return YAlignmentEnum.Top;

                case AlinhamentoVertical.Centro:
                    return YAlignmentEnum.Middle;

                case AlinhamentoVertical.Base:
                    return YAlignmentEnum.Bottom;
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Converts Millimeters to Point
        /// </summary>
        /// <param name="mm"></param>
        /// <returns></returns>
        public static float ToPoint(this float mm) => PointFactor * mm;

        /// <summary>
        /// Converts Millimeters to Point
        /// </summary>
        /// <param name="mm"></param>
        /// <returns></returns>
        public static double ToPoint(this double mm) => PointFactor * mm;

        /// <summary>
        /// Converts Point to Millimeters
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static SizeF ToPointMeasure(this SizeF s) => new SizeF(s.Width.ToPoint(), s.Height.ToPoint());

        public static RectangleF ToPointMeasure(this RectangleF r) => new RectangleF(r.X.ToPoint(), r.Y.ToPoint(), r.Width.ToPoint(), r.Height.ToPoint());

        public static PointF ToPointMeasure(this PointF r) => new PointF(r.X.ToPoint(), r.Y.ToPoint());

        #endregion Public Methods
    }
}