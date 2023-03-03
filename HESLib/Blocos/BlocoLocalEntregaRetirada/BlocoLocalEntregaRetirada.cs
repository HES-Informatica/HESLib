﻿using System;
using Extensions.BR;
using HES.Modelo;

namespace HES.Blocos
{
    abstract class BlocoLocalEntregaRetirada : BlocoBase
    {
        public LocalEntregaRetiradaViewModel Model { get; private set; }

        public BlocoLocalEntregaRetirada(DANFEModel viewModel, Estilo estilo, LocalEntregaRetiradaViewModel localModel) : base(viewModel, estilo)
        {
            Model = localModel ?? throw new ArgumentNullException(nameof(localModel));

            AdicionarLinhaCampos()
            .ComCampo(Extensions.Util.NomeRazaoSocial, Model.NomeRazaoSocial)
            .ComCampo(Extensions.Util.CnpjCpf, Model.CnpjCpf.FormatarCPFOuCNPJ(), AlinhamentoHorizontal.Centro)
            .ComCampo(Extensions.Util.InscricaoEstadual, Model.InscricaoEstadual, AlinhamentoHorizontal.Centro)
            .ComLarguras(0, 45F * Proporcao, 30F * Proporcao);

            AdicionarLinhaCampos()
            .ComCampo(Extensions.Util.Endereco, Model.Endereco)
            .ComCampo(Extensions.Util.BairroDistrito, Model.Bairro)
            .ComCampo(Extensions.Util.Cep, Model.Cep.FormatarCEP(), AlinhamentoHorizontal.Centro)
            .ComLarguras(0, 45F * Proporcao, 30F * Proporcao);

            AdicionarLinhaCampos()
            .ComCampo(Extensions.Util.Municipio, Model.Municipio)
            .ComCampo(Extensions.Util.UF, Model.Uf, AlinhamentoHorizontal.Centro)
            .ComCampo(Extensions.Util.FoneFax, Model.Telefone.FormatarTelefone(), AlinhamentoHorizontal.Centro)
            .ComLarguras(0, 7F * Proporcao, 30F * Proporcao);
        }

        public override PosicaoBloco Posicao => PosicaoBloco.Topo;

    }
}