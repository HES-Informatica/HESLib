using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;
using Extensions.ComplexText;
using Extensions.Files;
using HES.Blocos;
using HES.Bytes;
using HES.Documents.Contents.Fonts;
using HES.Files;
using HES.Modelo;

namespace HES
{
    public class DANFE
    {
        #region Internal Fields

        internal static StandardType1Font FonteItalico;

        internal static StandardType1Font FonteNegrito;

        internal static StandardType1Font FonteRegular;

        internal List<BlocoBase> Blocos;

        internal bool disposedValue = false;

        internal StandardType1Font.FamilyEnum FonteFamilia;

        internal object ObjetoLogo = null;

        #endregion Internal Fields

        #region Internal Properties

        internal BlocoCanhoto Canhoto { get; private set; }

        internal Estilo EstiloPadrao { get; private set; }

        internal BlocoIdentificacaoEmitente IdentificacaoEmitente { get; private set; }

        internal List<DanfePagina> Paginas { get; private set; }

        #endregion Internal Properties

        #region Internal Methods

        internal T AdicionarBloco<T>() where T : BlocoBase
        {
            var bloco = CriarBloco<T>();
            Blocos.Add(bloco);
            return bloco;
        }

        internal T AdicionarBloco<T>(Estilo estilo) where T : BlocoBase
        {
            var bloco = CriarBloco<T>(estilo);
            Blocos.Add(bloco);
            return bloco;
        }

        internal void AdicionarBloco(BlocoBase bloco) => Blocos.Add(bloco);

        /// <inheritdoc cref="AdicionarLogoImagem(Stream)"/>
        internal void AdicionarLogoImagem(string path)
        {
            if (path.IsNotValid()) throw new ArgumentException(nameof(path));
            AdicionarLogoImagem(new FileStream(path, FileMode.Open, FileAccess.Read));
        }

        internal void AdicionarLogoImagem(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0) throw new ArgumentException(nameof(imageBytes));
            AdicionarLogoImagem(new MemoryStream(imageBytes));

        }

        internal void AdicionarLogoPdf(byte[] pdfBytes)
        {
            if (pdfBytes == null || pdfBytes.Length == 0) throw new ArgumentException(nameof(pdfBytes));
            AdicionarLogoPdf(new MemoryStream(pdfBytes));

        }

        /// <inheritdoc cref="AdicionarLogoImagem(Stream)"/>
        internal void AdicionarLogoPdf(string path)
        {
            if (path.IsNotValid()) throw new ArgumentException(nameof(path));
            AdicionarLogoPdf(new FileStream(path, FileMode.Open, FileAccess.Read));
        }

        internal string AjustarCaminhoPDF(string FilePath, TipoDocumento? TipoDocumento = null)
        {
            FilePath = FilePath.IfBlank(Path.GetTempPath());
            if (!FilePath.IsPath())
            {
                FilePath = Path.Combine(Path.GetTempPath(), FilePath);
            }

            if (FilePath.IsDirectoryPath())
            {
                var n = (ViewModel?.ChaveAcesso).IfBlank(Path.GetDirectoryName(FilePath));
                if (n.IsNumber())
                {
                    if (TipoDocumento.HasValue == false)
                    {
                        n = GerarNomeUnico(n, ViewModel?.SequenciaCorrecao ?? 0);
                    }
                    else if (TipoDocumento.Value == Modelo.TipoDocumento.DANFE)
                    {
                        n = GerarNomePdfDANFE(n);
                    }
                    else
                    {
                        n = GerarNomePdfCCE(n, ViewModel?.SequenciaCorrecao ?? 0);
                    }
                }

                FilePath = Path.Combine(FilePath, $"{Path.GetFileNameWithoutExtension(n)}.pdf");
            }

            if (FilePath.IsFilePath())
            {
                var file = new FileInfo(FilePath).ReplaceExtension("pdf").FullName;
                file.WriteDebug("PDF FilePath");
                return file;
            }
            else
            {
                throw new ArgumentException("Caminho do documento inválido", nameof(FilePath));
            }
        }

        internal T CriarBloco<T>() where T : BlocoBase => (T)Activator.CreateInstance(typeof(T), ViewModel, EstiloPadrao);

        internal T CriarBloco<T>(Estilo estilo) where T : BlocoBase => (T)Activator.CreateInstance(typeof(T), ViewModel, estilo);

        internal DanfePagina CriarPagina(PdfFile F)
        {
            DanfePagina p = new DanfePagina(this, F);
            Paginas.Add(p);
            p.DesenharBlocos(Paginas.Count == 1);

            if (LicenseKey != "hes.com.br".GenerateLicenseKey())
            {
                p.DesenharCreditos("Impresso com H&S Technologies - hes.com.br");
            }
            else if (Creditos.IsValid())
            {
                p.DesenharCreditos(Creditos);
            }

            // Ambiente de homologação
            // 7. O DANFE emitido para representar NF-e cujo uso foi autorizado em ambiente de
            // homologação sempre deverá conter a frase “SEM VALOR FISCAL” no quadro “Informações
            // Complementares” ou em marca d’água destacada.
            if (ViewModel.TipoAmbiente == Esquemas.NFe.TAmb.Homologacao)
                p.DesenharAvisoHomologacao();

            return p;
        }

        internal void PreencherNumeroFolhas()
        {
            int nFolhas = Paginas.Count;
            for (int i = 0; i < Paginas.Count; i++)
            {
                Paginas[i].DesenhaNumeroPaginas(i + 1, nFolhas);
            }
        }

        #endregion Internal Methods

        #region Public Constructors

        public DANFE(string xmlNFePath, string xmlCCePath, string logoPath) : this(DANFEModel.CriarDeArquivoXml(xmlNFePath, xmlCCePath), logoPath)
        {
        }

        public DANFE(string xmlNFePath, string logoPath) : this(DANFEModel.CriarDeArquivoXml(xmlNFePath, null), logoPath)
        {
        }

        public DANFE(string xmlNFePath) : this(DANFEModel.CriarDeArquivoXml(xmlNFePath, null), "")
        {
        }

        public DANFE(DANFEModel viewModel) : this(viewModel, "")
        {
        }

        public DANFE(DANFEModel viewModel, string logoPath)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            AdicionarLogo(logoPath);
        }

        public DANFE(DANFEModel viewModel, byte[] logoPath)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            AdicionarLogo(logoPath);
        }

        public DANFE(byte[] xml, byte[] cce = null, byte[] logo = null) : this(DANFEModel.CriarDeBytesXml(xml, cce), logo)
        {

        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Empresa responsavel por gerar o DANFE
        /// </summary>
        public static string Autor { get; set; } = Extensions.Util.GetCompanyName();

        /// <summary>
        /// Nota rodapé adicionada em cada folha do DANFE
        /// </summary>
        public static string Creditos { get; set; }

        /// <summary>
        /// Chave de licença para customização dos <see cref="Creditos"/>.
        /// </summary>
        public static string LicenseKey { get; set; }

        /// <summary>
        /// Informações extraidas do XML da nota fiscal e da carta de correção
        /// </summary>
        public DANFEModel ViewModel { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Gera um nome padronizado para CCE
        /// </summary>
        /// <param name="ChaveNFe"></param>
        /// <param name="SequenciaEvento"></param>
        /// <returns></returns>
        public static string GerarNomePdfCCE(string ChaveNFe, int SequenciaEvento) => $"DANFE-{(ChaveNFe).RemoveMask()}-CCE-{SequenciaEvento}.pdf";

        /// <summary>
        /// Gera um nome padronizado para o DANFE
        /// </summary>
        /// <param name="ChaveNFe"></param>
        /// <returns></returns>
        public static string GerarNomePdfDANFE(string ChaveNFe) => $"DANFE-{(ChaveNFe).RemoveMask()}.pdf";

        public static string GerarNomeUnico(string ChaveNFe, int SequenciaEvento) => $"DANFE-CCE-{(ChaveNFe).RemoveMask()}-Seq-{SequenciaEvento}.pdf";

        /// <inheritdoc cref="GerarPDF(string, string, string, DirectoryInfo)"/>
        public static IEnumerable<FileInfo> GerarPDF(string PathNFe, string PathCCe, DirectoryInfo outputPath) => GerarPDF(PathNFe, PathCCe, null, outputPath);

        /// <inheritdoc cref="GerarPDF(string, string, string, DirectoryInfo)"/>
        public static FileInfo GerarPDF(string PathNFe, DirectoryInfo outputPath) => GerarPDF(PathNFe, null, null, outputPath).FirstOrDefault();

        /// <summary>
        /// Gera um DANFE em PDF com as informações fornecidas
        /// </summary>
        /// <param name="PathNFe">Caminho do XML da nota fiscal</param>
        /// <param name="PathCCe">Caminho do XML da carta de correção</param>
        /// <param name="outputPath">Caminho de saída dos arquivos</param>
        /// <param name="PathLogo">Caminho com o logo da empresa responsavel pela NFe</param>
        /// <returns>uma lista de <see cref="FileInfo"/> com os caminhos dos arquivos gerados</returns>
        public static IEnumerable<FileInfo> GerarPDF(string PathNFe, string PathCCe, string PathLogo, DirectoryInfo outputPath) => new DANFE(PathNFe, PathCCe, PathLogo).Gerar(outputPath);

        /// <summary>
        /// Adiciona um logo JPG ou PDF ao DANFE.
        /// </summary>
        /// <param name="logoPath">Caminho para o PDF ou JPG</param>
        public void AdicionarLogo(string logoPath)
        {
            if (logoPath.IsFilePath())
            {
                var f = new FileInfo(logoPath);
                if (f.Exists)
                {

                    if (f.Extension.FlatEqual(".pdf"))
                    {
                        AdicionarLogoPdf(logoPath);
                    }
                    else if (f.Extension.IsAny(StringComparison.OrdinalIgnoreCase, ".jpg", ".jpeg"))
                    {
                        AdicionarLogoImagem(logoPath);
                    }
                    else
                    {
                        throw new Exception("Arquivo não é um PDF ou JPEG");
                    }
                }
                else
                {
                    throw new FileNotFoundException("Arquivo não existe");
                }
            }
        }

        /// <summary>
        /// Adiciona um logo ao DANFE.
        /// </summary>
        /// <param name="logo">O logo a ser adicionado.</param>
        /// <param name="isPdf">Indica se o logo é um arquivo PDF.</param>
        public void AdicionarLogo(byte[] logo, bool isPdf = false)
        {
            if (logo != null && logo.Length > 1)
            {
                if (isPdf)
                {
                    AdicionarLogoPdf(logo);
                }
                else
                {
                    AdicionarLogoImagem(logo);
                }
            }
        }

        /// <summary>
        /// Adiciona um logo a NFe
        /// </summary>
        /// <param name="stream"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void AdicionarLogoImagem(System.IO.Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            var img = Documents.Contents.Entities.Image.Get(stream) ?? throw new InvalidOperationException("O logotipo não pode ser carregado, certifique-se que a imagem esteja no formato JPEG não progressivo.");
            ObjetoLogo = img;
        }

        /// <inheritdoc cref="AdicionarLogoImagem(Stream)"/>
        public void AdicionarLogoPdf(System.IO.Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            ObjetoLogo = new Files.PdfFile(new HES.Bytes.Stream(stream)).Document.Pages[0];
        }

        public PdfFile Gerar(TipoDocumento TipoDocumento)
        {
            var oldTipo = ViewModel.TipoDocumento;

            ViewModel.TipoDocumento = TipoDocumento;

            var OutputFile = new PdfFile();

            // De acordo com o item 7.7, a fonte deve ser Times New Roman ou Courier New.
            FonteFamilia = StandardType1Font.FamilyEnum.Times;
            FonteRegular = new StandardType1Font(OutputFile.Document, FonteFamilia, false, false);
            FonteNegrito = new StandardType1Font(OutputFile.Document, FonteFamilia, true, false);
            FonteItalico = new StandardType1Font(OutputFile.Document, FonteFamilia, false, true);



            EstiloPadrao = Extensions.Util.CriarEstilo();

            Paginas = new List<DanfePagina>();

            Blocos = new List<BlocoBase>();

            Canhoto = CriarBloco<BlocoCanhoto>();
            IdentificacaoEmitente = AdicionarBloco<BlocoIdentificacaoEmitente>();

            if (ObjetoLogo is Documents.Contents.Entities.Image img)
            {
                IdentificacaoEmitente.Logo = img.ToXObject(OutputFile.Document);
            }
            else if (ObjetoLogo is Documents.Page pg)
            {
                IdentificacaoEmitente.Logo = pg.ToXObject(OutputFile.Document);
            }
            else if (ObjetoLogo is Documents.Contents.xObjects.XObject oo)
            {
                IdentificacaoEmitente.Logo = (Documents.Contents.xObjects.XObject)oo.Clone(OutputFile.Document);
            }

            ObjetoLogo = IdentificacaoEmitente.Logo;

            AdicionarBloco<BlocoDestinatarioRemetente>();
            AdicionarBloco<BlocoDadosAdicionais>(Extensions.Util.CriarEstilo(tFonteCampoConteudo: 8));

            if (ViewModel.TipoDocumento == TipoDocumento.DANFE)
            {
                if (ViewModel.LocalRetirada != null && ViewModel.ExibirBlocoLocalRetirada)
                    AdicionarBloco<BlocoLocalRetirada>();

                if (ViewModel.LocalEntrega != null && ViewModel.ExibirBlocoLocalEntrega)
                    AdicionarBloco<BlocoLocalEntrega>();

                if (ViewModel.Duplicatas.Count > 0)
                    AdicionarBloco<BlocoDuplicataFatura>();

                AdicionarBloco<BlocoCalculoImposto>(ViewModel.Orientacao == Orientacao.Paisagem ? EstiloPadrao : Extensions.Util.CriarEstilo(4.75F));
                AdicionarBloco<BlocoTransportador>();

                if (ViewModel.CalculoIssqn.Mostrar)
                    AdicionarBloco<BlocoCalculoIssqn>();

                var tabela = new TabelaProdutosServicos(ViewModel, EstiloPadrao);

                while (true)
                {
                    DanfePagina p = CriarPagina(OutputFile);

                    tabela.SetPosition(p.RetanguloCorpo.Location);
                    tabela.SetSize(p.RetanguloCorpo.Size);
                    tabela.Draw(p.Gfx);

                    p.Gfx.Stroke();
                    p.Gfx.Flush();

                    if (tabela.CompletamenteDesenhada) break;
                }
            }
            else if (ViewModel.TipoDocumento == TipoDocumento.CCE)
            {
                AdicionarBloco<BlocoCondicaoCCE>(Extensions.Util.CriarEstilo(tFonteCampoConteudo: 10));

                const int FirstCount = 3500;
                int OtherCount = FirstCount + 2550;

                var textopagina = "";
                int size = FirstCount;
                foreach (Paragraph par in new StructuredText(ViewModel.TextoCorrecao))
                {
                    foreach (var s in par)
                    {
                        var frase = s.ToString() + (par.Last() == s ? Environment.NewLine : " ");
                        if (textopagina.Length + frase.Length < size)
                        {
                            textopagina += frase;
                        }
                        else
                        {
                            var p = CriarPagina(OutputFile);
                            var correcao = new TextoSimples(EstiloPadrao, textopagina)
                            {
                                TamanhoFonte = 12
                            };
                            correcao.SetPosition(p.RetanguloCorpo.Location);
                            correcao.SetSize(p.RetanguloCorpo.Size);
                            correcao.Draw(p.Gfx);
                            p.Gfx.Stroke();
                            p.Gfx.Flush();
                            textopagina = "";
                            size = OtherCount;
                        }
                    }
                }
            }

            PreencherNumeroFolhas();

            //metadata do PDF
            var info = OutputFile.Document.Information;
            info[new Objects.PdfName("ChaveAcesso")] = ViewModel.ChaveAcesso.Chave;
            info[new Objects.PdfName("TipoDocumento")] = $"{TipoDocumento}";
            info[new Objects.PdfName("XML")] = ViewModel.XML;
            info.CreationDate = ViewModel.DataHoraEmissao;
            info.Creator = $"{Extensions.Util.GetAssemblyName()?.Name} {Extensions.Util.GetAssemblyName()?.Version} - Disponível em https://github.com/hes-informatica/HESLib";
            info.Author = Autor;

            info.Subject = ViewModel.TipoDocumento == TipoDocumento.DANFE ? "Documento Auxiliar da NFe" : "Carta de Correção Eletrônica";
            info.Title = $"{ViewModel.TipoDocumento}";


            ViewModel.TipoDocumento = oldTipo;
            return OutputFile;
        }

        /// <summary>
        /// Gera os PDFs no diretório especificado em <paramref name="path"/>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnumerable<FileInfo> Gerar(DirectoryInfo path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            path.CreateDirectoryIfNotExists();

            var nfe = AjustarCaminhoPDF(path.FullName, TipoDocumento.DANFE);

            yield return Gerar(nfe, TipoDocumento.DANFE);

            if (ViewModel?.PossuiCCE ?? false)
            {
                var cce = AjustarCaminhoPDF(path.FullName, TipoDocumento.CCE);
                yield return Gerar(cce, TipoDocumento.CCE);
            }
        }

        public FileInfo Gerar(string FilePath) => Gerar(FilePath, ViewModel.TipoDocumento);

        /// <summary>
        /// Gera um DANFE ou uma CCE em um arquivo PDF especificado em <paramref name="FilePath"/>
        /// </summary>
        /// <param name="FilePath">Caminho do arquivo</param>
        /// <param name="TipoDocumento">Tipo do documento (DANFE ou CCE)</param>
        /// <returns></returns>
        public FileInfo Gerar(string FilePath, TipoDocumento TipoDocumento) => Gerar(FilePath, TipoDocumento, SerializationModeEnum.Incremental);

        public FileInfo Gerar(string FilePath, TipoDocumento TipoDocumento, SerializationModeEnum serializationMode)
        {
            using (var v = Gerar(TipoDocumento))
            {
                return v.Save(AjustarCaminhoPDF(FilePath), serializationMode);
            }
        }

        public byte[] GerarBytes(TipoDocumento TipoDocumento) => Gerar(TipoDocumento).ToBytes(SerializationModeEnum.Incremental);

        public FileInfo GerarUnico(string outputPath) => GerarUnico(outputPath, SerializationModeEnum.Incremental);

        public FileInfo GerarUnico(string outputPath, SerializationModeEnum serializationMode)
        {
            using (var x = GerarUnico())
            {
                return x.Save(AjustarCaminhoPDF(outputPath), serializationMode);
            }
        }

        public PdfFile GerarUnico()
        {
            var danfe = Gerar(TipoDocumento.DANFE);
            if (ViewModel?.PossuiCCE ?? false)
            {
                var cce = Gerar(TipoDocumento.CCE);

                return Extensions.Util.MergePDF(danfe, cce);
            }
            else
            {
                return danfe;
            }
        }

        public byte[] GerarUnicoBytes() => GerarUnico().ToBytes(SerializationModeEnum.Incremental);

        #endregion Public Methods
    }
}