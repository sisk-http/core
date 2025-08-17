# Integração do Sisk Cadente e abstração para Sisk Core

Implemente o projeto presente em `.\cadente\Sisk.Cadente.CoreEngine` para implementar todos os membros de `HttpServerEngine`. Todos os membros e propriedades das classes abstratas devem ser implementadas.

Siga o exemplo da implementação de um `HttpServerEngine` abaixo:
    
    .\src\Http\Abstractions\HttpListenerAbstractEngine.cs

No momento, não é necessário implementar web-sockets, pois o Sisk Cadente não suporta ele ainda nativamente.

Implemente, para o Sisk Cadente, as abstrações:

- HttpServerEngine
- HttpServerEngineContext
- HttpServerEngineContextRequest
- HttpServerEngineContextResponse

Importante: não toque em outros projetos, apenas em `Sisk.Cadente.CoreEngine`. Se algum problema impossibilitar você de continuar a integração, pare a integração.

Você precisará ler toda a API do projeto Sisk Cadente disponível em:

    .\cadente\Sisk.Cadente

E API de integração da engine:

    .\src\Http\Abstractions

Boa sorte.