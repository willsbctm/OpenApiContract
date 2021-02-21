# OpenApiContract
Lib para testar integrações do ponto de vista do cliente através do documento open api

### Exemplo de uso:
```c#
var handler = A.Fake<InterceptorFakeHandler>(x => x.CallsBaseMethods());
A.CallTo(() => handler.FakeSend(A<HttpRequestMessage>._))
  .Returns(new InterceptedResponse
  {
      Key = chave,
      HttpResponse = new HttpResponseMessage
      {
          StatusCode = HttpStatusCode.OK,
          Content = content
      }
  });
_ = await httpClient.GetAsync($"{host}/pet");

var call = handler.GetCall(chave);
call.Should().SatisfyEspecification(documentoOpenApi, "/pet/{petId}", HttpStatusCode.OK);
```

### Créditos:
Código baseado no projeto [Swashbuckle.AspNetCore.ApiTesting](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/master/src/Swashbuckle.AspNetCore.ApiTesting)