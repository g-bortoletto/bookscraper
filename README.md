# Unecont BookScraper

Aplicativo de linha de comando (.NET) que realiza scraping de livros do site Books to Scrape, filtra por critérios configuráveis e salva a saída em JSON e XML. Também envia o resultado para um endpoint HTTP (httpbin.org) para demonstração e logging.

## Pré-requisitos

- .NET 10 SDK instalado
- Acesso à internet (para scraping e POST de teste)

## Como executar

No PowerShell, a partir da raiz do repositório:

- Executar diretamente com o SDK:
  - `dotnet run --project src/Unecont.BookScraper.App -c Release`
- Ou executar o binário (após compilar):
  - `bin/BookScraper.exe`

Os arquivos de saída e logs são gerados no diretório atual de execução.

## Configuração (appsettings.json, vars de ambiente, CLI)

Arquivo base: `src/Unecont.BookScraper.App/appsettings.json`

- `Categories` (array de strings): Categorias para fazer scraping. Os nomes devem corresponder às chaves do mapeamento interno:
  - "Travel"
  - "Mystery"
  - "Historical Fiction"
  - "Sequential Art"
  - "Classics"
  - "Philosophy"
  - "Romance"
  - "Womens Fiction"
  - "Fiction"
  - "Childrens"
  - "Religion"
  - "Nonfiction"
  - "Music"
  - "Default"
  - "Science Fiction"
  - "Sports and Games"
  - "Add a comment"
  - "Fantasy"
  - "New Adult"
  - "Young Adult"
  - "Science"
  - "Poetry"
  - "Paranormal"
  - "Art"
  - "Psychology"
  - "Autobiography"
  - "Parenting"
  - "Adult Fiction"
  - "Humor"
  - "Horror"
  - "History"
  - "Food and Drink"
  - "Christian Fiction"
  - "Business"
  - "Biography"
  - "Thriller"
  - "Contemporary"
  - "Spirituality"
  - "Academic"
  - "Self Help"
  - "Historical"
  - "Christian"
  - "Suspense"
  - "Short Stories"
  - "Novels"
  - "Health"
  - "Politics"
  - "Cultural"
  - "Erotica"
  - "Crime"
- `Filters`:
  - `MinPrice` (decimal, opcional): preço mínimo.
  - `MaxPrice` (decimal, opcional): preço máximo.
  - `Rating` (inteiro 1–5, opcional): somente livros com essa classificação exata. Se um filtro não for informado, ele é ignorado.
- `Serilog`: Configuração de logs (console e arquivo diário em `Logs/bookscraper-.log`).

Também é possível sobrescrever configurações via:

- Variáveis de ambiente (padrão do .NET):
  - `Filters__MinPrice=10.0`
  - `Filters__MaxPrice=40.0`
  - `Filters__Rating=5`
  - `Categories__0=Mystery` `Categories__1=Fiction` `Categories__2=Nonfiction`
- Parâmetros de linha de comando (padrão do .NET):
  - `dotnet run --project src/Unecont.BookScraper.App -- Filters:Rating=3 Filters:MinPrice=15`

## O que o programa faz

1. Lê `Categories` da configuração e monta as URLs das categorias no Books to Scrape.
2. Descobre todas as páginas de cada categoria e realiza scraping em paralelo.
3. Para cada livro, extrai: `Title`, `Price`, `Rating`, `Category`, `Url`.
4. Aplica os filtros configurados (`MinPrice`, `MaxPrice`, `Rating`).
5. Serializa o resultado consolidado:
   - `out/books.json` (JSON indentado)
   - `out/books.xml` (estrutura equivalente)
6. Envia o objeto `Books` via HTTP POST para `https://httpbin.org/post` e registra no log o status e um resumo do payload.

## Saídas esperadas

- Arquivos:
  - `out/books.json`
  - `out/books.xml`
- Logs:
  - Console com mensagens de progresso e o `STATUS` do POST.
  - Arquivo de log diário em `Logs/bookscraper-YYYYMMDD.log`.
- POST de teste:
  - Deve retornar `200 OK` de `httpbin.org` com o eco do JSON enviado.

## Observações

- Os nomes das categorias devem existir no mapeamento interno (`ScrapingHelper.CategoryMap`). Caso informe um nome inexistente, a categoria é ignorada.
- `Rating` é comparado por igualdade (ex.: `Rating = 4` retorna apenas livros com 4 estrelas).
- Os caminhos de saída são relativos ao diretório de execução.

## Estrutura relevante

- App: `src/Unecont.BookScraper.App`
  - Entrada: `Program.cs`
  - Configuração: `appsettings.json`
- Core: `src/Unecont.BookScraper.Core`
  - Scraping: `Services/PageScraper.cs`
  - Cliente HTTP: `Services/BookClient.cs` (POST em `https://httpbin.org/post`)
  - Modelos/DTOs: `Models/Book.cs`
  - Utilitários: `Helpers/*`
