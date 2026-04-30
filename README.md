# 📅 API de Agendamento de Recursos - SACR

Esta API foi desenvolvida para gerenciar o agendamento de recursos (como salas, equipamentos e hardware), oferecendo controle completo de reservas com validação inteligente de conflitos de horários e integração com banco de dados.

## 🚀 Tecnologias Utilizadas
* **ASP.NET Core Web API**: Estrutura principal da aplicação.
* **Entity Framework Core**: ORM para persistência e manipulação de dados.
* **LINQ**: Utilizado para filtragem e lógica de validação de sobreposição de horários.
* **DTOs (Data Transfer Objects)**: Garantem a segurança e a separação entre a camada de dados e a camada de exibição.

## 🛠️ Funcionalidades e Endpoints
A `AgendamentoController` expõe os seguintes recursos:

### 1. Listar Agendamentos
* **Endpoint:** `GET /api/Agendamento`
* **Descrição:** Retorna todos os agendamentos registrados no sistema.

### 2. Consultar por ID
* **Endpoint:** `GET /api/Agendamento/{id}`
* **Descrição:** Busca os detalhes de um agendamento específico. Retorna `404 Not Found` caso o ID não exista.

### 3. Criar Novo Agendamento
* **Endpoint:** `POST /api/Agendamento`
* **Regras de Negócio:**
    * **Obrigatoriedade:** Nome do recurso e Responsável são campos obrigatórios.
    * **Consistência:** A data de término deve ser estritamente maior que a data de início.
    * **Validação de Conflito:** O sistema verifica automaticamente se o recurso já possui uma reserva no intervalo solicitado, impedindo agendamentos duplicados.

### 4. Atualizar Registro
* **Endpoint:** `PUT /api/Agendamento/{id}`
* **Descrição:** Atualiza os dados de uma reserva existente. As validações de conflito de horário também são aplicadas aqui, ignorando o próprio registro que está sendo editado.

### 5. Excluir Agendamento
* **Endpoint:** `DELETE /api/Agendamento/{id}`
* **Descrição:** Remove permanentemente um agendamento do sistema.

---

## 📋 Exemplo de Estrutura JSON (DTO)
Para interagir com os endpoints de criação e atualização, utilize o seguinte modelo:
```json
{
  "id": 0,
  "recursoNome": "Sala de Reuniões 01",
  "recursoTipo": "Ambiente",
  "dataInicio": "2026-05-15T09:00:00",
  "dataFim": "2026-05-15T11:00:00",
  "responsavel": "Marilene",
  "departamento": "TI",
  "status": "Confirmado"
}
```
## 🏃 Instruções para Execução

### Pré-requisitos
* **SDK .NET 6.0** ou superior.
* **Ferramenta de banco de dados**: SQLite, SQL Server ou similar, conforme a configuração do projeto.

### Passo a Passo

1. **Clonar o Repositório:**
   
```bash
   git clone [https://github.com/seu-usuario/seu-repositorio.git](https://github.com/seu-usuario/seu-repositorio.git)
   cd seu-repositorio
```

   ### 2. Restaurar Dependências
```bash
dotnet restore
```

### 3. Atualizar Banco de Dados
*Caso utilize o Entity Framework Migrations:*
```bash
dotnet ef database update
```
### 4. Rodar a Aplicação
```bash
dotnet run --project ApiAgendamento
```
### 5. Testar via Swagger
Acesse a documentação interativa através do navegador:
`https://localhost:7075/swagger` (ou a porta indicada no console de execução).

---
## 🛡️ Diferenciais do Projeto

*   **Validação de Sobreposição:** Implementação de lógica robusta que impede que dois eventos ocupem o mesmo recurso simultaneamente.
*   **Mapeamento de Entidades:** Uso estratégico de **DTOs** para evitar a exposição direta do modelo de banco de dados (Entidade), garantindo maior segurança.
*   **Tratamento de Erros Semânticos:** Respostas HTTP claras (como `200 OK`, `400 BadRequest` e `404 NotFound`) com mensagens descritivas para facilitar o consumo da API.
*   **Resiliência do Servidor (Erro 500):** Implementação de blocos `try-catch` em todos os endpoints da Controller, garantindo que falhas inesperadas ou erros de concorrência no banco de dados sejam tratados, retornando um status `500 Internal Server Error` padronizado em vez de interromper a execução da API.

