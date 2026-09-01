# VelsatBackendAPI

## Requisitos
- Docker

## Variables de entorno

| Variable | Descripción | Obligatoria |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Cadena de conexión MySQL (base `gts`) | Recomendada (si no se define, usa el valor de `appsettings.json` dentro de la imagen) |
| `ConnectionStrings__SecondConnection` | Cadena de conexión MySQL (base `dbv16_01`) | Recomendada |
| `ConnectionStrings__ThirdConnection` | Cadena de conexión MySQL (servidor 66.240.210.125, sin cambios) | Recomendada |
| `settings__secretkey` | Clave usada para firmar/validar los JWT | Recomendada |
| `Firebase__CredentialsPath` | Ruta al archivo de credenciales de Firebase (solo si se usa) | Opcional |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución (`Production`, `Development`, etc.) | Opcional |

`appsettings.json` **no se commitea** (ver `.gitignore`). Antes de compilar localmente o dentro de la imagen, copia `VelsatBackendAPI/appsettings.json.example` a `VelsatBackendAPI/appsettings.json` y completa los valores, o bien confía únicamente en las variables de entorno de arriba.

## Build de la imagen

Ejecutar desde la raíz del repositorio (el Dockerfile referencia rutas relativas a este contexto):

```bash
docker build -t velsat-backend-api -f VelsatBackendAPI/Dockerfile .
```

## Run del contenedor

```bash
docker run -d \
  --name velsat-backend-api \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="server=mysql-gts;port=3306;database=gts;uid=gtsuser;password=velsat2027;Charset=utf8mb4;Pooling=true;Allow User Variables=True;MinimumPoolSize=5;MaximumPoolSize=200;ConnectionTimeout=60;ConnectionLifeTime=300;ConnectionReset=true;DefaultCommandTimeout=120;" \
  -e ConnectionStrings__SecondConnection="server=mysql-gts;port=3306;database=dbv16_01;uid=gtsuser;password=velsat2027;Charset=utf8mb4;Pooling=true;Allow User Variables=True;MinimumPoolSize=5;MaximumPoolSize=200;ConnectionTimeout=60;ConnectionLifeTime=300;ConnectionReset=true;DefaultCommandTimeout=120;" \
  -e ConnectionStrings__ThirdConnection="server=66.240.210.125;port=3306;database=gts;uid=velsatuser;password=velsatAPI23;Charset=utf8mb4;Pooling=true;Allow User Variables=True;MinimumPoolSize=5;MaximumPoolSize=200;ConnectionTimeout=60;ConnectionLifeTime=300;ConnectionReset=true;DefaultCommandTimeout=120;" \
  -e settings__secretkey="TU_SECRET_KEY" \
  -e ASPNETCORE_ENVIRONMENT="Production" \
  -v /ruta/en/el/host/firebase-credentials.json:/app/firebase-credentials.json:ro \
  velsat-backend-api
```

Notas:
- `mysql-gts` debe resolver a la IP/servicio MySQL correcto (entrada DNS, `--add-host`, o si MySQL corre en otro contenedor, conectar ambos a la misma red de Docker con `docker network create` / `docker network connect`).
- El montaje de `firebase-credentials.json` es opcional: el repo no incluye ese archivo y actualmente no hay código que lo consuma activamente, pero `appsettings.json` sí referencia `Firebase:CredentialsPath`. Si en el futuro se activa la integración con Firebase, monta el archivo real en `/app/firebase-credentials.json` (ruta relativa al `WORKDIR /app` del contenedor) o cambia la ruta con `-e Firebase__CredentialsPath=/ruta/absoluta/firebase-credentials.json`.
- El contenedor escucha en `0.0.0.0:8080` (configurado vía `ASPNETCORE_URLS` en el Dockerfile). Publica el puerto que necesites con `-p <puerto_host>:8080`.
