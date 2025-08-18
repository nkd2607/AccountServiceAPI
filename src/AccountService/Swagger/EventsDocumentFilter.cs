using AccountService.Domain.Events;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class RabbitMqEventsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Extensions.Add("x-rabbitmq-events", new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["AccountOpened"] = new Microsoft.OpenApi.Any.OpenApiString("Публикуется при создании нового счета"),
            ["MoneyCredited"] = new Microsoft.OpenApi.Any.OpenApiString("Публикуется при начислении кредита на счет"),
            ["MoneyDebited"] = new Microsoft.OpenApi.Any.OpenApiString("Публикуется при списании со счета"),
            ["TransferCompleted"] = new Microsoft.OpenApi.Any.OpenApiString("Публикуется по завершении транзакции"),
            ["InterestAccrued"] = new Microsoft.OpenApi.Any.OpenApiString("Публикуется при начислении процентов"),
            ["ClientBlocked"] = new Microsoft.OpenApi.Any.OpenApiString("Публикуется при заморозке счета клиента"),
            ["ClientUnblocked"] = new Microsoft.OpenApi.Any.OpenApiString("Публикуется при разморозке счета клиента")
        });

        context.SchemaGenerator.GenerateSchema(typeof(AccountOpened), context.SchemaRepository);
        context.SchemaGenerator.GenerateSchema(typeof(MoneyCredited), context.SchemaRepository);
        context.SchemaGenerator.GenerateSchema(typeof(MoneyDebited), context.SchemaRepository);
        context.SchemaGenerator.GenerateSchema(typeof(TransferCompleted), context.SchemaRepository);
        context.SchemaGenerator.GenerateSchema(typeof(InterestAccrued), context.SchemaRepository);
        context.SchemaGenerator.GenerateSchema(typeof(ClientBlocked), context.SchemaRepository);
        context.SchemaGenerator.GenerateSchema(typeof(ClientUnblocked), context.SchemaRepository);
    }
}