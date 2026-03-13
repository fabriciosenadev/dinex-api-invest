# DinEx API Invest

API do DinEx para controle de investimentos com foco em rastreabilidade de carteira, extrato e base para imposto de renda.

## Visao Geral

O backend foi construido em .NET com arquitetura em camadas para separar regras de negocio, persistencia e exposicao HTTP.

Objetivos principais:
- registrar movimentacoes de investimento;
- importar extrato de planilhas da B3;
- manter carteira consolidada por usuario;
- aplicar eventos corporativos manuais;
- gerar base de apoio para imposto de renda;
- garantir rastreabilidade de operacoes via logs estruturados.

## Arquitetura

Estrutura da solution:

- `src/Api`: camada HTTP (controllers, contracts, pipeline, Swagger).
- `src/Service`: casos de uso (commands/queries, dispatcher, regras de aplicacao).
- `src/Infra`: persistencia e implementacoes de repositorios.
- `src/Core`: entidades, enums, resultados e contratos de dominio.
- `src/Tests`: testes automatizados.

Relacoes entre projetos:
- `Core -> Infra`
- `Infra -> Service`
- `Service -> Api`

## Modulos Funcionais

- **Usuarios**
  - cadastro
  - autenticacao e refresh
  - ativacao de conta
  - troca e reset de senha

- **Movimentacoes e Carteira**
  - registro manual de compra/venda
  - consolidacao da carteira por ativo
  - resumo para imposto de renda
  - reconciliacao de carteira via planilha de posicao

- **Extrato**
  - importacao de planilhas B3
  - lancamento manual de entradas no livro de extrato
  - limpeza de extrato/carteira por usuario

- **Eventos Corporativos**
  - cadastro, edicao e exclusao de eventos
  - aplicacao na carteira consolidada

- **Catalogo de Ativos**
  - cadastro e manutencao de metadados de ativos

## Padrao de Resposta

A API utiliza `OperationResult` para padronizar retorno de sucesso e erro, com mapeamento para codigos HTTP na `MainController`.

## Logs e Rastreabilidade

A API possui logging estruturado com:
- logs de request/response no pipeline;
- correlacao por `TraceId` e `UserId`;
- rastreio de comandos/queries no dispatcher;
- niveis separados para desenvolvimento e producao.

## Execucao Local

No diretorio `DinExApi`:

```bash
dotnet build .\DinExApi.sln
dotnet test .\DinExApi.sln
dotnet run --project .\src\Api
```

## Documentacao de Endpoints

Ha um arquivo de chamadas HTTP com exemplos em:

- `src/Api/DinExApi.Api.http`

Tambem e possivel validar os endpoints pela interface Swagger quando a API estiver em execucao.

## Qualidade

- testes automatizados cobrindo regras de dominio e aplicacao;
- separacao clara entre regras de negocio e infraestrutura;
- suporte a provedores de banco relacionais com migracao automatica no startup.
