# PaymentsService

API REST (.NET 10) para gestionar **billeteras** y **transferencias de saldo** entre ellas. Este es el servicio que opera actualmente en producción y sobre el que trabajarás durante la prueba práctica.

## Arquitectura

Clean Architecture con cuatro proyectos:

```
src/
  PaymentsService.Domain/         Entidades del dominio
  PaymentsService.Application/    Casos de uso, servicios y puertos (interfaces)
  PaymentsService.Infrastructure/ EF Core, repositorios, DbContext (SQL Server)
  PaymentsService.Api/            Minimal API, autenticacion JWT, endpoints
tests/
  PaymentsService.Tests/          xUnit + NSubstitute
db/
  schema.sql                      Esquema y datos de ejemplo
```

## Requisitos

- .NET 10 SDK
- SQL Server (local o vía Docker)

## Puesta en marcha

1. Levantar SQL Server:

   ```bash
   docker compose up -d db
   ```

2. Crear el esquema y los datos de ejemplo:

   ```bash
   sqlcmd -S localhost,1433 -U sa -P "Your_strong_password123" -C -i db/schema.sql -d master
   ```

   > La base `Payments` debe existir antes de correr el script, o créala con `CREATE DATABASE Payments;`.

3. Aplicar migraciones:

```bash
   dotnet ef database update --project src/PaymentsService.Infrastructure --startup-project src/PaymentsService.Api
```

4. Ejecutar la API:

   ```bash
   dotnet run --project src/PaymentsService.Api
   ```

5. Ejecutar las pruebas:

   ```bash
   dotnet test
   ```

## Autenticación

Los endpoints de creación y transferencia requieren un bearer token. Para obtener uno en desarrollo:

```bash
curl -X POST http://localhost:5080/v1/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"support"}'
```

Usa el `access_token` devuelto en el header `Authorization: Bearer <token>`.

## Endpoints

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/v1/auth/token` | No | Emite un token de desarrollo |
| POST | `/v1/wallets` | Sí | Crea una billetera |
| GET | `/v1/wallets/{id}` | Sí | Consulta una billetera |
| POST | `/v1/transfers` | Sí | Transfiere saldo entre billeteras |
| POST | `/v1/transfers/{id}/reversal` | Sí | Revierte una transferencia existente
| GET | `/v1/wallets/{id}/movements` | No | Historial de movimientos |

### Ejemplos

Crear billetera:

```json
POST /v1/wallets
{ "documentId": "0102030405", "name": "Ana Torres", "initialBalance": 100 }
```

Transferir:

```json
POST /v1/transfers
{ "fromWalletId": 1, "toWalletId": 2, "amount": 25.5 }
```
Revertir una transferencia:

```json
POST /v1/transfers/1/reversal
```

Errores posibles en la reversa:

| Codigo | Descripcion |
|--------|-------------|
| 404 | Movimiento o billetera no encontrada |
| 409 | La transferencia ya fue revertida |
| 422 | Saldo insuficiente para revertir |
| 400 | Solo se pueden revertir movimientos de débito |

## Cambios recientes

- `Balance` y `Amount` cambiados de `double` a `decimal` en toda la solucion para garantizar precision en operaciones monetarias.
- Nuevo campo `ReversedMovementId` en `Movement` para control de reversas.
- Nuevo endpoint `POST /v1/transfers/{id}/reversal` para revertir transferencias.
- Nuevos metodos `GetByIdAsync` y `UpdateAsync` en `IMovementRepository`.