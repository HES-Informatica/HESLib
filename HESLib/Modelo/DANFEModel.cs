using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Extensions;
using Extensions.BR;
using HES.Esquemas;
using HES.Esquemas.NFe;

namespace HES.Modelo
{
    /// <summary>
    /// Modelo de dados utilizado para o DANFE ou CC.
    /// </summary>
    public class DANFEModel

    {
        #region Private Fields

        private float _Margem;
        private int _quant_canhoto;

        #endregion Private Fields

        #region Private Methods

        private static EmpresaViewModel CreateEmpresaFrom(Empresa empresa)
        {
            EmpresaViewModel model = new EmpresaViewModel
            {
                RazaoSocial = empresa.xNome,
                CnpjCpf = empresa.CNPJ.IfBlank(empresa.CPF),
                Ie = empresa.IE,
                IeSt = empresa.IEST,
                Email = empresa.email
            };

            var end = empresa.Endereco;

            if (end != null)
            {
                model.Street = end.xLgr;
                model.Number = end.nro;
                model.Neighborhood = end.xBairro;
                model.City = end.xMun;
                model.StateCode = end.UF;
                model.PostalCode = end.CEP;
                model.Telefone = end.fone;
                model.Complement = end.xCpl;
            }

            if (empresa is Emitente emit)
            {
                model.IM = emit.IM;
                model.CRT = emit.CRT;
                model.NomeFantasia = emit.xFant;
            }

            return model;
        }

        private static LocalEntregaRetiradaViewModel CreateLocalRetiradaEntrega(LocalEntregaRetirada local)
        {
            var m = new LocalEntregaRetiradaViewModel()
            {
                NomeRazaoSocial = local.xNome,
                CnpjCpf = local.CNPJ.IfBlank(local.CPF),
                InscricaoEstadual = local.IE,
                Telefone = local.fone,
                Neighborhood = local.xBairro,
                City = local.xMun,
                StateCode = local.UF,
                PostalCode = local.CEP,
                Number = local.nro,
                Complement = local.xCpl,
                Street = local.xLgr
            };

            return m;
        }

        private static DANFEModel CriarDeArquivoXmlInternal(TextReader nfeReader, TextReader cceReader = null)
        {
            try
            {
                string xmlContent = nfeReader.ReadToEnd();
                
                ProcNFe procNfe = null;
                NFe nfeSimples = null;
                ProcEventoNFe cce = null;
                
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);
                
                string rootElementName = xmlDoc.DocumentElement.LocalName;
                
                if (rootElementName == "nfeProc")
                {
                    XmlSerializer nfeSerializer = new XmlSerializer(typeof(ProcNFe));
                    using (StringReader sr = new StringReader(xmlContent))
                    {
                        procNfe = (ProcNFe)nfeSerializer.Deserialize(sr);
                    }
                    
                    if (cceReader != null)
                    {
                        XmlSerializer cceSerializer = new XmlSerializer(typeof(ProcEventoNFe));
                        cce = (ProcEventoNFe)cceSerializer.Deserialize(cceReader);
                    }
                    
                    return Create(procNfe, cce);
                }
                else if (rootElementName == "NFe")
                {
                    XmlSerializer nfeSerializer = new XmlSerializer(typeof(NFe));
                    using (StringReader sr = new StringReader(xmlContent))
                    {
                        nfeSimples = (NFe)nfeSerializer.Deserialize(sr);
                    }
                    
                    if (cceReader != null)
                    {
                        XmlSerializer cceSerializer = new XmlSerializer(typeof(ProcEventoNFe));
                        cce = (ProcEventoNFe)cceSerializer.Deserialize(cceReader);
                    }
                    
                    return Create(nfeSimples, cce);
                }
                else
                {
                    throw new XmlException($"Elemento raiz '{rootElementName}' não reconhecido. Esperado 'nfeProc' ou 'NFe'.");
                }
            }
            catch (Exception e)
            {
                if (e.InnerException is XmlException ex)
                {
                    throw new Exception(string.Format("Não foi possível interpretar o Xml. Linha {0} Posição {1}.", ex.LineNumber, ex.LinePosition));
                }

                throw new XmlException("O Xml não parece ser uma NF-e válida.", e);
            }
          

        }

        #endregion Private Methods

        #region Internal Methods

        internal static CalculoImpostoViewModel CriarCalculoImpostoViewModel(ICMSTotal i) => new CalculoImpostoViewModel()
        {
            ValorAproximadoTributos = i.vTotTrib,
            BaseCalculoIcms = i.vBC,
            ValorIcms = i.vICMS,
            BaseCalculoIcmsSt = i.vBCST,
            ValorIcmsSt = i.vST,
            ValorTotalProdutos = i.vProd,
            ValorFrete = i.vFrete,
            ValorSeguro = i.vSeg,
            Desconto = i.vDesc,
            ValorII = i.vII,
            ValorIpi = i.vIPI,
            ValorPis = i.vPIS,
            ValorCofins = i.vCOFINS,
            OutrasDespesas = i.vOutro,
            ValorTotalNota = i.vNF,
            vFCPUFDest = i.vFCPUFDest,
            vICMSUFDest = i.vICMSUFDest,
            vICMSUFRemet = i.vICMSUFRemet
        };

        internal static void ExtrairDatas(DANFEModel model, InfNFe infNfe)
        {
            var ide = infNfe.ide;

            if (infNfe.Versao.Maior >= 3)
            {
                if (ide.dhEmi.HasValue) model.DataHoraEmissao = ide.dhEmi?.DateTimeOffsetValue.DateTime;
                if (ide.dhSaiEnt.HasValue) model.DataSaidaEntrada = ide.dhSaiEnt?.DateTimeOffsetValue.DateTime;

                if (model.DataSaidaEntrada.HasValue)
                {
                    model.HoraSaidaEntrada = model.DataSaidaEntrada?.TimeOfDay;
                }
            }
            else
            {
                model.DataHoraEmissao = ide.dEmi;
                model.DataSaidaEntrada = ide.dSaiEnt;

                if (Extensions.Util.IsValid(ide.hSaiEnt))
                {
                    model.HoraSaidaEntrada = TimeSpan.Parse(ide.hSaiEnt);
                }
            }
        }

        #endregion Internal Methods

        #region Public Fields

        public static readonly IEnumerable<FormaEmissao> FormasEmissaoSuportadas = new FormaEmissao[] { FormaEmissao.Normal, FormaEmissao.ContingenciaSVCAN, FormaEmissao.ContingenciaSVCRS };

        #endregion Public Fields

        #region Public Constructors

        public DANFEModel()
        {
            QuantidadeCanhotos = 1;
            Margem = 4;
            Orientacao = Orientacao.Retrato;
            CalculoImposto = new CalculoImpostoViewModel();
            Emitente = new EmpresaViewModel();
            Destinatario = new EmpresaViewModel();
            Duplicatas = new List<DuplicataViewModel>();
            Produtos = new List<ProdutoViewModel>();
            Transportadora = new TransportadoraViewModel();
            CalculoIssqn = new CalculoIssqnViewModel();
            NotasFiscaisReferenciadas = new List<string>();
        }

        #endregion Public Constructors

        #region Public Properties

        public string XML { get; private set; }

        /// <summary>
        /// View Model do bloco Cálculo do Imposto
        /// </summary>
        public CalculoImpostoViewModel CalculoImposto { get; set; }

        /// <summary>
        /// View Model do Bloco Cálculo do Issqn
        /// </summary>
        public CalculoIssqnViewModel CalculoIssqn { get; set; }

        /// <summary>
        /// Chave de Acesso
        /// </summary>
        public ChaveNFe ChaveAcesso { get; set; } = new ChaveNFe();

        /// <summary>
        /// Código do status da resposta, cStat, do elemento infProt.
        /// </summary>
        public int? CodigoStatusReposta { get; set; }

        public DateTime? ContingenciaDataHora { get; set; }

        public string ContingenciaJustificativa { get; set; }

        /// <summary>
        /// Tag xCont
        /// </summary>
        public string Contrato { get; set; }

        public DateTime? DataHoraCorrecao { get; set; }

        /// <summary>
        /// <para>Data e Hora de emissão do Documento Fiscal</para>
        /// <para>Tag dhEmi ou dEmi</para>
        /// </summary>
        public DateTime? DataHoraEmissao { get; set; }

        /// <summary>
        /// <para>Data de Saída ou da Entrada da Mercadoria/Produto</para>
        /// <para>Tag dSaiEnt e dhSaiEnt</para>
        /// </summary>
        public DateTime? DataSaidaEntrada { get; set; }

        /// <summary>
        /// Descrição do status da resposta, xMotivo, do elemento infProt.
        /// </summary>
        public string DescricaoStatusReposta { get; set; }

        /// <summary>
        /// Dados do Destinatário
        /// </summary>
        public EmpresaViewModel Destinatario { get; set; }

        /// <summary>
        /// Faturas da Nota Fiscal
        /// </summary>
        public List<DuplicataViewModel> Duplicatas { get; set; }

        /// <summary>
        /// Dados do Emitente
        /// </summary>
        public EmpresaViewModel Emitente { get; set; }

        /// <summary>
        /// Exibi o bloco "Informações do local de entrega" quando o elemento "entrega" estiver disponível.
        /// </summary>
        public bool ExibirBlocoLocalEntrega { get; set; } = true;

        /// <summary>
        /// Exibi o bloco "Informações do local de retirada" quando o elemento "retirada" estiver disponível.
        /// </summary>
        public bool ExibirBlocoLocalRetirada { get; set; } = true;

        /// <summary>
        /// Exibi os valores do ICMS Interestadual e Valor Total dos Impostos no bloco Cálculos do Imposto.
        /// </summary>
        public bool ExibirIcmsInterestadual { get; set; } = true;

        /// <summary>
        /// Exibi os valores do PIS e COFINS no bloco Cálculos do Imposto.
        /// </summary>
        public bool ExibirPisConfins { get; set; } = true;

        /// <summary>
        /// <para>Hora de Saída ou da Entrada da Mercadoria/Produto</para>
        /// <para>Tag dSaiEnt e hSaiEnt</para>
        /// </summary>
        public TimeSpan? HoraSaidaEntrada { get; set; }

        /// <summary>
        /// <para>Informações adicionais de interesse do Fisco</para>
        /// <para>Tag infAdFisco</para>
        /// </summary>
        public string InformacoesAdicionaisFisco { get; set; }

        /// <summary>
        /// <para>Informações Complementares de interesse do Contribuinte</para>
        /// <para>Tag infCpl</para>
        /// </summary>
        public string InformacoesComplementares { get; set; }

        public bool IsPaisagem => Orientacao == Orientacao.Paisagem;

        public bool IsRetrato => Orientacao == Orientacao.Retrato;

        public LocalEntregaRetiradaViewModel LocalEntrega { get; set; }

        public LocalEntregaRetiradaViewModel LocalRetirada { get; set; }

        /// <summary>
        /// Magens horizontais e verticais do DANFE.
        /// </summary>
        public float Margem
        {
            get => _Margem;
            set
            {
                if (value >= 2 && value <= 5)
                    _Margem = value;
                else
                    throw new ArgumentOutOfRangeException("A margem deve ser entre 2 e 5.");
            }
        }

        public bool MostrarCalculoIssqn { get; set; }

        /// <summary>
        /// <para>Descrição da Natureza da Operação</para>
        /// <para>Tag natOp</para>
        /// </summary>
        public string NaturezaOperacao { get; set; }

        /// <summary>
        /// Tag xNEmp
        /// </summary>
        public string NotaEmpenho { get; set; }

        /// <summary>
        /// Informações de Notas Fiscais referenciadas que serão levadas no texto adicional.
        /// </summary>
        public List<string> NotasFiscaisReferenciadas { get; set; }

        public Orientacao Orientacao { get; set; } = Orientacao.Retrato;

        /// <summary>
        /// Tag xPed
        /// </summary>
        public string Pedido { get; set; }

        public bool PossuiCCE => TextoCorrecao.IsValid() && SequenciaCorrecao > 0;

        /// <summary>
        /// Exibe o Nome Fantasia, caso disponível, ao invés da Razão Social no quadro identificação
        /// do emitente.
        /// </summary>
        public bool PreferirEmitenteNomeFantasia { get; set; } = true;

        /// <summary>
        /// Produtos da Nota Fiscal
        /// </summary>
        public List<ProdutoViewModel> Produtos { get; set; }

        /// <summary>
        /// Numero do protocolo com sua data e hora
        /// </summary>
        public string ProtocoloAutorizacao { get; set; }

        public string ProtocoloCorrecao { get; set; }

        /// <summary>
        /// Quantidade de canhotos a serem impressos.
        /// </summary>
        public int QuantidadeCanhotos
        {
            get => _quant_canhoto;
            set
            {
                if (value >= 0 && value <= 2)
                    _quant_canhoto = value;
                else
                    throw new ArgumentOutOfRangeException("A quantidade de canhotos deve de 0 a 2.");
            }
        }

        public int SequenciaCorrecao { get; set; }

        public virtual string TextoAdicional
        {
            get
            {
                if (TipoDocumento == TipoDocumento.CCE)
                {
                    return $"PROTOCOLO: {ProtocoloCorrecao}{Environment.NewLine}DATA/HORA: {DataHoraCorrecao:dd/MM/yyyy HH:mm:ss}{Environment.NewLine}EVENTO: {SequenciaCorrecao}";
                }
                else
                {
                    StringBuilder sb = new StringBuilder();

                    if (!string.IsNullOrEmpty(InformacoesComplementares))
                        sb.AppendChaveValor("Inf. Contribuinte", InformacoesComplementares).Replace(";", "\r\n");

                    if (!string.IsNullOrEmpty(Destinatario.Email))
                    {
                        // Adiciona um espaço após a virgula caso necessário, isso facilita a quebra
                        // de linha.
                        var destEmail = Regex.Replace(Destinatario.Email, @"(?<=\S)([,;])(?=\S)", "$1 ").Trim(new char[] { ' ', ',', ';' });
                        sb.AppendChaveValor("Email do Destinatário", destEmail);
                    }

                    if (!string.IsNullOrEmpty(InformacoesAdicionaisFisco))
                        sb.AppendChaveValor("Inf. fisco", InformacoesAdicionaisFisco);

                    if (!string.IsNullOrEmpty(Pedido) && !Utils.StringContemChaveValor(InformacoesComplementares, "Pedido", Pedido))
                        sb.AppendChaveValor("Pedido", Pedido);

                    if (!string.IsNullOrEmpty(Contrato) && !Utils.StringContemChaveValor(InformacoesComplementares, "Contrato", Contrato))
                        sb.AppendChaveValor("Contrato", Contrato);

                    if (!string.IsNullOrEmpty(NotaEmpenho))
                        sb.AppendChaveValor("Nota de Empenho", NotaEmpenho);

                    foreach (var nfref in NotasFiscaisReferenciadas)
                    {
                        if (sb.Length > 0) sb.Append(" ");
                        sb.Append(nfref);
                    }

                    #region NT 2013.003 Lei da Transparência

                    if (CalculoImposto.ValorAproximadoTributos.HasValue && (string.IsNullOrEmpty(InformacoesComplementares) ||
                        !Regex.IsMatch(InformacoesComplementares, @"((valor|vlr?\.?)\s+(aprox\.?|aproximado)\s+(dos\s+)?(trib\.?|tributos))|((trib\.?|tributos)\s+(aprox\.?|aproximado))", RegexOptions.IgnoreCase)))
                    {
                        if (sb.Length > 0) sb.Append("\r\n");
                        sb.Append("Valor Aproximado dos Tributos: ");
                        sb.Append(CalculoImposto.ValorAproximadoTributos.FormatarMoeda());
                    }

                    #endregion NT 2013.003 Lei da Transparência

                    return sb.ToString();
                }
            }
        }

        public virtual string TextoAdicionalFisco
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (TipoEmissao == FormaEmissao.ContingenciaSVCAN || TipoEmissao == FormaEmissao.ContingenciaSVCRS)
                {
                    sb.Append("Contingência ");

                    if (TipoEmissao == FormaEmissao.ContingenciaSVCAN)
                        sb.Append("SVC-AN");

                    if (TipoEmissao == FormaEmissao.ContingenciaSVCRS)
                        sb.Append("SVC-RS");

                    if (ContingenciaDataHora.HasValue)
                    {
                        sb.Append($" - {ContingenciaDataHora.FormatarDataBrasileira()}");
                    }

                    if (Extensions.Util.IsValid(ContingenciaJustificativa))
                    {
                        sb.Append($" - {ContingenciaJustificativa}");
                    }

                    sb.Append(".");
                }

                return sb.ToString();
            }
        }

        public string TextoCondicaoDeUso { get; internal set; }
        public string TextoCorrecao { get; set; }
        public virtual string TextoRecebimento => $"Recebemos de {Emitente.RazaoSocial} os produtos e/ou serviços constantes na Nota Fiscal Eletrônica indicada {(Orientacao == Orientacao.Retrato ? "abaixo" : "ao lado")}. Emissão: {DataHoraEmissao.Formatar()} Valor Total: R$ {CalculoImposto.ValorTotalNota.Formatar()} Destinatário: {Destinatario.RazaoSocial}";

        /// <summary>
        /// Tipo de Ambiente
        /// </summary>
        public TAmb TipoAmbiente { get; set; } = TAmb.Producao;

        public TipoDocumento TipoDocumento { get; set; } = TipoDocumento.DANFE;

        /// <summary>
        /// Tipo de emissão
        /// </summary>
        public FormaEmissao TipoEmissao { get; set; }

        /// <summary>
        /// <para>Tipo de Operação - 0-entrada / 1-saída</para>
        /// <para>Tag tpNF</para>
        /// </summary>
        public Tipo TipoNF { get; set; }

        /// <summary>
        /// Dados da Transportadora
        /// </summary>
        public TransportadoraViewModel Transportadora { get; set; }

        #endregion Public Properties

        #region Public Methods

        public static DANFEModel Create(ProcNFe procNfe, ProcEventoNFe procEventoNFe = null)
        {
            var model = new DANFEModel();

            model.XML = procNfe.CreateXML().OuterXml;

            model.TipoEmissao = procNfe.NFe.infNFe.ide.tpEmis;

            if (procNfe.NFe.infNFe.ide.mod != 55)
            {
                throw new NotSupportedException("Somente o mod==55 está implementado.");
            }

            if (!FormasEmissaoSuportadas.Contains(model.TipoEmissao))
            {
                throw new NotSupportedException($"O tpEmis {procNfe.NFe.infNFe.ide.tpEmis} não é suportado.");
            }

            model.Orientacao = procNfe.NFe.infNFe.ide.tpImp == 1 ? Orientacao.Retrato : Orientacao.Paisagem;
            model.ChaveAcesso = procNfe.NFe.infNFe.Id;
            model.CodigoStatusReposta = procNfe.protNFe.infProt.cStat;
            model.DescricaoStatusReposta = procNfe.protNFe.infProt.xMotivo;

            model.TipoAmbiente = procNfe.NFe.infNFe.ide.tpAmb;
            model.ChaveAcesso.Nota = procNfe.NFe.infNFe.ide.nNF;
            model.ChaveAcesso.Serie = procNfe.NFe.infNFe.ide.serie;
            model.NaturezaOperacao = procNfe.NFe.infNFe.ide.natOp;
            model.TipoNF = procNfe.NFe.infNFe.ide.tpNF;

            model.Emitente = CreateEmpresaFrom(procNfe.NFe.infNFe.emit);
            model.Destinatario = CreateEmpresaFrom(procNfe.NFe.infNFe.dest);

            // Local retirada e entrega
            if (procNfe.NFe.infNFe.retirada != null)
            {
                model.LocalRetirada = CreateLocalRetiradaEntrega(procNfe.NFe.infNFe.retirada);
            }

            if (procNfe.NFe.infNFe.entrega != null)
            {
                model.LocalEntrega = CreateLocalRetiradaEntrega(procNfe.NFe.infNFe.entrega);
            }

            model.NotasFiscaisReferenciadas = procNfe.NFe.infNFe.ide.NFref.Select(x => x.ToString()).ToList();

            // Informações adicionais de compra
            if (procNfe.NFe.infNFe.compra != null)
            {
                model.Contrato = procNfe.NFe.infNFe.compra.xCont;
                model.NotaEmpenho = procNfe.NFe.infNFe.compra.xNEmp;
                model.Pedido = procNfe.NFe.infNFe.compra.xPed;
            }

            foreach (var det in procNfe.NFe.infNFe.det)
            {
                var produto = new ProdutoViewModel
                {
                    Codigo = det.prod.cProd,
                    Descricao = det.prod.xProd,
                    Ncm = det.prod.NCM,
                    Cfop = det.prod.CFOP,
                    Unidade = det.prod.uCom,
                    Quantidade = det.prod.qCom,
                    ValorUnitario = det.prod.vUnCom,
                    ValorTotal = det.prod.vProd,
                    InformacoesAdicionais = det.infAdProd
                };

                var imposto = det.imposto;

                if (imposto != null)
                {
                    if (imposto.ICMS != null)
                    {
                        var icms = imposto.ICMS.ICMS;

                        if (icms != null)
                        {
                            produto.ValorIcms = icms.vICMS;
                            produto.BaseIcms = icms.vBC;
                            produto.AliquotaIcms = icms.pICMS;
                            produto.OCst = icms.orig + icms.CST + icms.CSOSN;
                        }
                    }

                    if (imposto.IPI != null)
                    {
                        var ipi = imposto.IPI.IPITrib;

                        if (ipi != null)
                        {
                            produto.ValorIpi = ipi.vIPI;
                            produto.AliquotaIpi = ipi.pIPI;
                        }
                    }
                }

                model.Produtos.Add(produto);
            }

            if (procNfe.NFe.infNFe.cobr != null)
            {
                foreach (var item in procNfe.NFe.infNFe.cobr.dup)
                {
                    var duplicata = new DuplicataViewModel
                    {
                        Numero = item.nDup,
                        Valor = item.vDup,
                        Vecimento = item.dVenc
                    };

                    model.Duplicatas.Add(duplicata);
                }
            }

            model.CalculoImposto = CriarCalculoImpostoViewModel(procNfe.NFe.infNFe.total.ICMSTot);

            var issqnTotal = procNfe.NFe.infNFe.total.ISSQNtot;

            if (issqnTotal != null)
            {
                var c = model.CalculoIssqn;
                c.InscricaoMunicipal = procNfe.NFe.infNFe.emit.IM;
                c.BaseIssqn = issqnTotal.vBC;
                c.ValorTotalServicos = issqnTotal.vServ;
                c.ValorIssqn = issqnTotal.vISS;
                c.Mostrar = true;
            }

            var transp = procNfe.NFe.infNFe.transp;
            var transportadora = transp.transporta;
            var transportadoraModel = model.Transportadora;

            transportadoraModel.ModalidadeFrete = (int)transp.modFrete;

            if (transp.veicTransp != null)
            {
                transportadoraModel.VeiculoUf = transp.veicTransp.UF;
                transportadoraModel.CodigoAntt = transp.veicTransp.RNTC;
                transportadoraModel.Placa = transp.veicTransp.placa;
            }

            if (transportadora != null)
            {
                transportadoraModel.RazaoSocial = transportadora.xNome;
                transportadoraModel.StateCode = transportadora.UF;
                transportadoraModel.CnpjCpf = transportadora.CNPJ.IfBlank(transportadora.CPF);
                transportadoraModel.Street = transportadora.xEnder;
                transportadoraModel.City = transportadora.xMun;
                transportadoraModel.Ie = transportadora.IE;
            }

            var vol = transp.vol.FirstOrDefault();

            if (vol != null)
            {
                transportadoraModel.QuantidadeVolumes = vol.qVol;
                transportadoraModel.Especie = vol.esp;
                transportadoraModel.Marca = vol.marca;
                transportadoraModel.Numeracao = vol.nVol;
                transportadoraModel.PesoBruto = vol.pesoB;
                transportadoraModel.PesoLiquido = vol.pesoL;
            }

            var infAdic = procNfe.NFe.infNFe.infAdic;
            if (infAdic != null)
            {
                model.InformacoesComplementares = procNfe.NFe.infNFe.infAdic.infCpl;
                model.InformacoesAdicionaisFisco = procNfe.NFe.infNFe.infAdic.infAdFisco;
            }

            var infoProto = procNfe.protNFe.infProt;

            model.ProtocoloAutorizacao = string.Format(Utils.Cultura, "{0} - {1}", infoProto.nProt, infoProto.dhRecbto.DateTimeOffsetValue.DateTime);

            ExtrairDatas(model, procNfe.NFe.infNFe);

            // Contingência SVC-AN e SVC-RS
            if (model.TipoEmissao == FormaEmissao.ContingenciaSVCAN || model.TipoEmissao == FormaEmissao.ContingenciaSVCRS)
            {
                model.ContingenciaDataHora = procNfe.NFe.infNFe.ide.dhCont?.DateTimeOffsetValue.DateTime;
                model.ContingenciaJustificativa = procNfe.NFe.infNFe.ide.xJust;
            }

            //implementação da carta de correção
            if (procEventoNFe != null)
            {
                model.SequenciaCorrecao = procEventoNFe.Evento?.InfEvento?.NSeqEvento ?? 0;
                model.ProtocoloCorrecao = procEventoNFe.RetEvento?.InfEvento?.NProt;
                model.DataHoraCorrecao = procEventoNFe.Evento?.InfEvento?.DhEvento;

                var det = procEventoNFe.Evento?.InfEvento?.DetEvento;
                if (det != null)
                {
                    model.TextoCondicaoDeUso = det.XCondUso?.Replace(";", Environment.NewLine);
                    model.TextoCorrecao = det.XCorrecao;
                }
            }

            return model;
        }

        public static DANFEModel Create(NFe nfe, ProcEventoNFe procEventoNFe = null)
        {
            var model = new DANFEModel();

            var xmlDoc = new XmlDocument();
            var nfeElement = xmlDoc.CreateElement("NFe", Namespaces.NFe);
            xmlDoc.AppendChild(nfeElement);
            model.XML = xmlDoc.OuterXml;

            model.TipoEmissao = nfe.infNFe.ide.tpEmis;

            if (nfe.infNFe.ide.mod != 55)
            {
                throw new NotSupportedException("Somente o mod==55 está implementado.");
            }

            if (!FormasEmissaoSuportadas.Contains(model.TipoEmissao))
            {
                throw new NotSupportedException($"O tpEmis {nfe.infNFe.ide.tpEmis} não é suportado.");
            }

            model.Orientacao = nfe.infNFe.ide.tpImp == 1 ? Orientacao.Retrato : Orientacao.Paisagem;
            model.ChaveAcesso = nfe.infNFe.Id;
            
            model.TipoAmbiente = nfe.infNFe.ide.tpAmb;
            model.ChaveAcesso.Nota = nfe.infNFe.ide.nNF;
            model.ChaveAcesso.Serie = nfe.infNFe.ide.serie;
            model.NaturezaOperacao = nfe.infNFe.ide.natOp;
            model.TipoNF = nfe.infNFe.ide.tpNF;

            model.Emitente = CreateEmpresaFrom(nfe.infNFe.emit);
            model.Destinatario = CreateEmpresaFrom(nfe.infNFe.dest);

            if (nfe.infNFe.retirada != null)
            {
                model.LocalRetirada = CreateLocalRetiradaEntrega(nfe.infNFe.retirada);
            }

            if (nfe.infNFe.entrega != null)
            {
                model.LocalEntrega = CreateLocalRetiradaEntrega(nfe.infNFe.entrega);
            }

            model.NotasFiscaisReferenciadas = nfe.infNFe.ide.NFref.Select(x => x.ToString()).ToList();

            if (nfe.infNFe.compra != null)
            {
                model.Contrato = nfe.infNFe.compra.xCont;
                model.NotaEmpenho = nfe.infNFe.compra.xNEmp;
                model.Pedido = nfe.infNFe.compra.xPed;
            }

            foreach (var det in nfe.infNFe.det)
            {
                var produto = new ProdutoViewModel
                {
                    Codigo = det.prod.cProd,
                    Descricao = det.prod.xProd,
                    Ncm = det.prod.NCM,
                    Cfop = det.prod.CFOP,
                    Unidade = det.prod.uCom,
                    Quantidade = det.prod.qCom,
                    ValorUnitario = det.prod.vUnCom,
                    ValorTotal = det.prod.vProd,
                    InformacoesAdicionais = det.infAdProd
                };

                var imposto = det.imposto;

                if (imposto != null)
                {
                    if (imposto.ICMS != null)
                    {
                        var icms = imposto.ICMS.ICMS;

                        if (icms != null)
                        {
                            produto.ValorIcms = icms.vICMS;
                            produto.BaseIcms = icms.vBC;
                            produto.AliquotaIcms = icms.pICMS;
                            produto.OCst = icms.orig + icms.CST + icms.CSOSN;
                        }
                    }

                    if (imposto.IPI != null)
                    {
                        var ipi = imposto.IPI.IPITrib;

                        if (ipi != null)
                        {
                            produto.ValorIpi = ipi.vIPI;
                            produto.AliquotaIpi = ipi.pIPI;
                        }
                    }
                }

                model.Produtos.Add(produto);
            }

            if (nfe.infNFe.cobr != null)
            {
                foreach (var item in nfe.infNFe.cobr.dup)
                {
                    var duplicata = new DuplicataViewModel
                    {
                        Numero = item.nDup,
                        Valor = item.vDup,
                        Vecimento = item.dVenc
                    };

                    model.Duplicatas.Add(duplicata);
                }
            }

            model.CalculoImposto = CriarCalculoImpostoViewModel(nfe.infNFe.total.ICMSTot);

            var issqnTotal = nfe.infNFe.total.ISSQNtot;

            if (issqnTotal != null)
            {
                var c = model.CalculoIssqn;
                c.InscricaoMunicipal = nfe.infNFe.emit.IM;
                c.BaseIssqn = issqnTotal.vBC;
                c.ValorTotalServicos = issqnTotal.vServ;
                c.ValorIssqn = issqnTotal.vISS;
                c.Mostrar = true;
            }

            var transp = nfe.infNFe.transp;
            var transportadora = transp.transporta;
            var transportadoraModel = model.Transportadora;

            transportadoraModel.ModalidadeFrete = (int)transp.modFrete;

            if (transp.veicTransp != null)
            {
                transportadoraModel.VeiculoUf = transp.veicTransp.UF;
                transportadoraModel.CodigoAntt = transp.veicTransp.RNTC;
                transportadoraModel.Placa = transp.veicTransp.placa;
            }

            if (transportadora != null)
            {
                transportadoraModel.RazaoSocial = transportadora.xNome;
                transportadoraModel.StateCode = transportadora.UF;
                transportadoraModel.CnpjCpf = transportadora.CNPJ.IfBlank(transportadora.CPF);
                transportadoraModel.Street = transportadora.xEnder;
                transportadoraModel.City = transportadora.xMun;
                transportadoraModel.Ie = transportadora.IE;
            }

            var vol = transp.vol.FirstOrDefault();

            if (vol != null)
            {
                transportadoraModel.QuantidadeVolumes = vol.qVol;
                transportadoraModel.Especie = vol.esp;
                transportadoraModel.Marca = vol.marca;
                transportadoraModel.Numeracao = vol.nVol;
                transportadoraModel.PesoBruto = vol.pesoB;
                transportadoraModel.PesoLiquido = vol.pesoL;
            }

            var infAdic = nfe.infNFe.infAdic;
            if (infAdic != null)
            {
                model.InformacoesComplementares = nfe.infNFe.infAdic.infCpl;
                model.InformacoesAdicionaisFisco = nfe.infNFe.infAdic.infAdFisco;
            }

            ExtrairDatas(model, nfe.infNFe);

            if (model.TipoEmissao == FormaEmissao.ContingenciaSVCAN || model.TipoEmissao == FormaEmissao.ContingenciaSVCRS)
            {
                model.ContingenciaDataHora = nfe.infNFe.ide.dhCont?.DateTimeOffsetValue.DateTime;
                model.ContingenciaJustificativa = nfe.infNFe.ide.xJust;
            }

            if (procEventoNFe != null)
            {
                model.SequenciaCorrecao = procEventoNFe.Evento?.InfEvento?.NSeqEvento ?? 0;
                model.ProtocoloCorrecao = procEventoNFe.RetEvento?.InfEvento?.NProt;
                model.DataHoraCorrecao = procEventoNFe.Evento?.InfEvento?.DhEvento;

                var det = procEventoNFe.Evento?.InfEvento?.DetEvento;
                if (det != null)
                {
                    model.TextoCondicaoDeUso = det.XCondUso?.Replace(";", Environment.NewLine);
                    model.TextoCorrecao = det.XCorrecao;
                }
            }

            return model;
        }
        /// <summary>
        /// Cria o modelo a partir de um arquivo xml.
        /// </summary>
        /// <param name="caminhoNFe"></param>
        /// <returns></returns>
        public static DANFEModel CriarDeArquivoXml(string caminhoNFe, string caminhoCCe)
        {
            using (var sr = new StreamReader(caminhoNFe, true))
            {
                if (caminhoCCe.IsFilePath() && File.Exists(caminhoCCe))
                {
                    using (var sr2 = new StreamReader(caminhoCCe, true))
                    {
                        return CriarDeArquivoXmlInternal(sr, sr2);
                    }
                }
                else
                {
                    return CriarDeArquivoXmlInternal(sr);
                }
            }
        }

        /// <summary>
        /// Cria o modelo a partir de um arquivo xml contido num stream.
        /// </summary>
        /// <param name="nfeStream"></param>
        /// <returns>Modelo</returns>
        public static DANFEModel CriarDeStreamXML(Stream nfeStream, Stream cceStream = null)
        {
            if (nfeStream == null) throw new ArgumentNullException(nameof(nfeStream));

            using (var sr = new StreamReader(nfeStream, true))
            {
                if (cceStream != null)
                {
                    using (var sr2 = new StreamReader(cceStream, true))
                    {
                        return CriarDeArquivoXmlInternal(sr, sr2);
                    }
                }
                else
                {
                    return CriarDeArquivoXmlInternal(sr);
                }
            }
        }



        /// <summary>
        /// Cria um objeto DANFEModel a partir de um array de bytes contendo o XML da NFe e, opcionalmente, o XML da CCe.
        /// </summary>
        /// <param name="nfeBytes">Array de bytes contendo o XML da NFe.</param>
        /// <param name="cceBytes">Array de bytes contendo o XML da CCe (opcional).</param>
        /// <returns>O objeto DANFEModel criado.</returns>
        public static DANFEModel CriarDeBytesXml(byte[] nfeBytes, byte[] cceBytes = null)
        {
            using (var nfeStream = new MemoryStream(nfeBytes))
            {
                using (var cceStream = cceBytes != null ? new MemoryStream(cceBytes) : null)
                {
                    return CriarDeStreamXML(nfeStream, cceStream);
                }
            }
        }


        /// <summary>
        /// Cria o modelo a partir de uma string xml.
        /// </summary>
        public static DANFEModel CriarDeStringXml(string nfeXML, string cceXML = null)
        {
            if (nfeXML.IsNotValid()) throw new ArgumentNullException(nameof(nfeXML));

            nfeXML = nfeXML.Trim();
            cceXML = cceXML?.Trim();
            using (var sr = new StringReader(nfeXML))
            {
                if (cceXML != null)
                {
                    using (var sr2 = new StringReader(cceXML))
                    {
                        return CriarDeArquivoXmlInternal(sr, sr2);
                    }
                }
                else
                {
                    return CriarDeArquivoXmlInternal(sr);
                }
            }
        }

        #endregion Public Methods
    }
}