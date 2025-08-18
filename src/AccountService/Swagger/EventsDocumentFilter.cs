using AccountService.Domain.Events;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class RabbitMqEventsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Extensions.Add("x-rabbitmq-events", new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["AccountOpened"] = new Microsoft.OpenApi.Any.OpenApiString("����������� ��� �������� ������ �����"),
            ["MoneyCredited"] = new Microsoft.OpenApi.Any.OpenApiString("����������� ��� ���������� ������� �� ����"),
            ["MoneyDebited"] = new Microsoft.OpenApi.Any.OpenApiString("����������� ��� �������� �� �����"),
            ["TransferCompleted"] = new Microsoft.OpenApi.Any.OpenApiString("����������� �� ���������� ����������"),
            ["InterestAccrued"] = new Microsoft.OpenApi.Any.OpenApiString("����������� ��� ���������� ���������"),
            ["ClientBlocked"] = new Microsoft.OpenApi.Any.OpenApiString("����������� ��� ��������� ����� �������"),
            ["ClientUnblocked"] = new Microsoft.OpenApi.Any.OpenApiString("����������� ��� ���������� ����� �������")
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