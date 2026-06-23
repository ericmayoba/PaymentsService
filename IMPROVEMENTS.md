# IMPROVEMENTS.md

## Propuesta de Observabilidad — PaymentsService

## 1. ¿Qué métricas, logs o alertas habrían detectado los incidentes antes?

**Bug #1 — Precisión decimal:** Agregar un log del balance calculado antes de guardarlo habría mostrado valores como `67.80000000000001` y nos habría alertado del problema sin necesidad de que un cliente lo reportara.

**Bug #3 — Inconsistencia de datos:** Un log simple por cada paso de la transferencia (débito aplicado, crédito aplicado) con un ID de operación habría permitido identificar rápidamente cuándo una transferencia quedó incompleta.

## 2. ¿Qué cambiarías para reducir el tiempo de detección y resolución?

- Agregar logs en los puntos críticos del flujo de transferencia para no depender de reportes de usuarios.
- Crear una consulta SQL programada que verifique diariamente que la suma de débitos sea igual a la suma de créditos.
- Definir un proceso claro de respuesta ante incidentes para que cualquier miembro del equipo pueda investigar y resolver sin escalar.

## Resumen

Los dos incidentes se habrían detectado mucho antes con logs básicos en el servicio de transferencias. La mejora más importante no es técnica sino de proceso: no esperar a que el cliente reporte el problema para empezar a buscar qué salió mal.