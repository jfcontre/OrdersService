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

---

## Instalación y Configuración

### 1. **Clonar el Repositorio**

```sh
 git clone https://github.com/jfcontre/OrdersService.git
 cd OrdersService
```

### 2. **Instalar AWS CLI**

Para gestionar los recursos de AWS localmente, instala **AWS CLI** siguiendo las instrucciones en [AWS CLI Installation](https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html).

Verifica la instalación ejecutando:

```sh
aws --version
```

### 3. **Configurar Credenciales de AWS**

Para trabajar localmente con **LocalStack**, configura credenciales falsas en `~/.aws/credentials`:

```ini
Archivo Config
[local]
aws_access_key_id = test
aws_secret_access_key = test
region = us-east-1

Archivo Credentials
[local]
aws_access_key_id = test
aws_secret_access_key = test
region = us-east-1
```

---

### 4. **Levantar el Entorno Local con Docker**

El `docker-compose.yml` se encarga de levantar **DynamoDB y LocalStack**.

```sh
docker-compose up -d
```

Verifica que los contenedores estén corriendo:

```sh
docker ps
```

---

### 5. **Crear las Colas de SQS Manualmente**

Ejecuta los siguientes comandos para crear las colas en LocalStack:

```sh
aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name ReceivedQueue
aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name InProcessQueue
aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name CompletedQueue
aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name CancelledQueue
```

Verifica que se hayan creado correctamente:

```sh
aws --endpoint-url=http://localhost:4566 sqs list-queues
```

---

### 6. **Ejecutar la API en Local**

```sh
dotnet run --project src/OrdersService.Api
```

La API estará disponible en:

```
http://localhost:5022/swagger/index.html
```

---

## **Uso de la API**

La API expone un **endpoint REST** para manejar órdenes de trabajo:

### **Crear una Orden (POST /orders)**

Ejemplo de **request**:

```json
{
  "descripcion": "Reparación de pantalla",
  "fechaRegistro": "2025-02-20T00:00:00Z",
  "fechaEntrega": "2025-02-25T00:00:00Z",
  "estado": "Recibida",
  "motivoCancelacion": ""
}
```

Ejemplo de **respuesta**:

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000"
}
```

El campo `motivoCancelacion` es obligatorio.

El campo `estado` es obligatorio.

---

## **Pruebas Unitarias**

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

## **Despliegue con Serverless Framework**

1. Instala Serverless Framework:

```sh
npm install -g serverless
```

2. Ejecuta el despliegue en AWS:

```sh
serverless deploy
```

3. Verifica el **API Gateway** generado:

```sh
serverless info
```

---

## **Manejo de Errores**

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

## **Escalabilidad y Optimización**

Para manejar alto volumen de solicitudes:

- Se usa **SQS para procesamiento asíncrono**.
- Se pueden utilizar **colas FIFO** para garantizar el orden.
- **Step Functions** permite dividir el procesamiento en pasos eficientes.
- Se puede configurar **Auto Scaling** en AWS Lambda.

---

## **Autor**

Desarrollado por [Jhon F. Contreras].

---

## **Licencia**

MIT License.


By JhonFC