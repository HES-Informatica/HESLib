# HES Danfe

HES Danfe é uma biblioteca em C# que permite a geração do DANFE em formato PDF. É um fork de DanfeSharp com algumas melhorias e implementações novas

A biblioteca PDF Clown é utilizada para a escrita dos arquivos em PDF.

Exemplo de uso:
```csharp

using HESDanfe;
using HESDanfe.Modelo;


var nfe = @"C:\NotasFiscais\XmlDistribuicao\00000000000000000000000000000000000000000000-procNFe.xml";
var cce = @"C:\NotasFiscais\XmlEventos\CartaDeCorrecao\00000000000000000000000000000000000000000000_110110_01-proceventonfe.xml";
var logo = @"C:\Logo\logo_nota_fiscal.jpg";


using (var d = new DANFE(nfe, cce, logo))
{
	foreach (var pdf in d.Gerar(new DirectoryInfo(@"C:\Teste\testeDANFE")))
	{

		// agora é so imprimir os PDfs ou enviar por email

	}

}
```


