# OrdersService - Microservicio Serverless para Procesamiento de Órdenes de Trabajo

## Descripción

Este proyecto es un microservicio **serverless** desarrollado en **.NET 9** que permite recibir y procesar órdenes de trabajo para un taller de servicio técnico. Las órdenes se almacenan en **Amazon DynamoDB** y se publican en **Amazon SQS**, según su estado.

## Tecnologías Utilizadas

- **.NET 9.0**
- **AWS DynamoDB** (Base de datos NoSQL)
- **AWS SQS** (Sistema de colas para procesamiento asíncrono)
- **AWS API Gateway** (Exposición de la API REST)
- **AWS Step Functions** (Orquestación de procesos)
- **LocalStack** (Para pruebas locales de servicios AWS)
- **Serverless Framework** (Infraestructura como código - IaC)
- **Docker & Docker Compose** (Para entorno de pruebas local)
- **AWS CLI** (Interactuar con servicios AWS)

---

## Instalación y Configuración

### 1. **Clonar el Repositorio**

```sh
 git clone https://github.com/jfcontre/OrdersService.git
 cd OrdersService
```

### 2. **Instalar AWS CLI**

Para gestionar recursos de AWS y emulaciones en LocalStack, instala **AWS CLI**:

- [Guía de instalación oficial](https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html)

Verifica la instalación:

```bash
aws --version
```

---

### 3. **Configurar Credenciales de AWS**

Para trabajar localmente con **LocalStack**, configura credenciales dummy:

**Archivo Credentials (`~/.aws/credentials`):**
```ini
[local]
aws_access_key_id = test
aws_secret_access_key = test
region = us-east-1
```

**Archivo Config (`~/.aws/config`):**
```ini
[profile local]
region = us-east-1
output = json
```

Luego, exporta el perfil:

```sh
export AWS_PROFILE=local
```

---

## Levantar el Entorno Local con Docker

El archivo `docker-compose.yml` se encarga de levantar **DynamoDB**, **LocalStack**, y la **API**. Asegúrate de que en tu `docker-compose.yml` tengas un mapeo de puertos, por ejemplo:

```yaml
services:
  orderservice-api:
    build:
      context: .
      dockerfile: src/OrdersService.Api/Dockerfile
    ports:
      - "8080:8080"  # Mapea el puerto 80 del contenedor al 8080 de la máquina host
    depends_on:
      - dynamodb
      - localstack
    environment:
      - ASPNETCORE_ENVIRONMENT=Development

  dynamodb:
    image: amazon/dynamodb-local
    ports:
      - "8000:8000"

  localstack:
    image: localstack/localstack
    ports:
      - "4566:4566"
      - "4571:4571"
    environment:
      - SERVICES=sqs
      - DEFAULT_REGION=us-east-1
```

Para iniciar todos los servicios, ejecuta:

```sh
docker-compose up -d
```

Verifica que los contenedores están corriendo:

```sh
docker ps
```

### Ingresar al contenedor LocalStack

Para crear colas y otros recursos **dentro** del contenedor LocalStack, primero confirma que el contenedor está en ejecución:
```sh
docker ps
```
Si deseas acceder a la terminal del contenedor, ejecuta:
```bash
docker exec -it <nombre_o_id_del_contenedor_localstack> /bin/bash
```
Una vez dentro del contenedor, podrás usar los comandos **AWS CLI** con las credenciales dummy.

---

### Crear las Colas de SQS Manualmente en LocalStack

Tienes dos opciones:

**Opción A: Ejecutar los comandos desde tu máquina host** (con `--endpoint-url` y perfil local):
```sh
aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name ReceivedQueue --profile local
aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name InProcessQueue --profile local
aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name CompletedQueue --profile local
aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name CancelledQueue --profile local
```

**Opción B: Ingresar al contenedor LocalStack** y ejecutar comandos AWS CLI:
```sh
docker exec -it <container_name> /bin/bash
# Ya dentro del contenedor:
aws sqs create-queue --queue-name ReceivedQueue
aws sqs create-queue --queue-name InProcessQueue
aws sqs create-queue --queue-name CompletedQueue
aws sqs create-queue --queue-name CancelledQueue
```

Verifica las colas:
```sh
aws sqs list-queues
```

---

## Ejecutar la API en Local

Si quieres levantar únicamente la API, sin Docker:
```sh
dotnet run --project src/OrdersService.Api
```

La API estará disponible en:
```
http://localhost:8080/swagger/index.html
```

(o el puerto que hayas mapeado en `docker-compose.yml`)

---

## Uso de la API

La API expone un **endpoint REST** para manejar órdenes de trabajo:

### Crear una Orden (POST /orders)

Ejemplo de request:
```json
{
  "descripcion": "Reparación de pantalla",
  "fechaRegistro": "2025-02-20T00:00:00Z",
  "fechaEntrega": "2025-02-25T00:00:00Z",
  "estado": "Recibida",
  "motivoCancelacion": ""
}
```

Ejemplo de respuesta:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000"
}
```

El campo `motivoCancelacion` es obligatorio. El campo `estado` es obligatorio.

---

## Pruebas Unitarias

Ejecuta las pruebas unitarias desde el directorio `tests/OrdersService.Tests/`:
```sh
cd tests/OrdersService.Tests/
dotnet test
```

Para ver la cobertura de pruebas:
```sh
dotnet test --collect:"XPlat Code Coverage"
```

---

## Manejo de Errores

Se implementó un middleware global para capturar errores y retornar respuestas estandarizadas en formato JSON.

Ejemplo de error controlado:
```json
{
  "error": {
    "code": "BadRequest",
    "message": "La fecha de entrega no puede ser anterior a la fecha de registro."
  }
}
```

---

## Despliegue con Serverless Framework
1. Instala Serverless Framework:
```sh
npm install -g serverless
```
2. Ejecuta el despliegue en AWS:
```sh
serverless deploy
```
3. Verifica el API Gateway generado:
```sh
serverless info
```

---

## Escalabilidad y Optimización

Para manejar alto volumen de solicitudes:
- Se usa **SQS** para procesamiento asíncrono.
- Se pueden utilizar **colas FIFO** para garantizar el orden.
- **Step Functions** para dividir el procesamiento en pasos.
- Configurar **Auto Scaling** en AWS Lambda.

---

## Autor
Desarrollado por [Jhon F. Contreras].

---

## Licencia
MIT License.


By JhonFC

