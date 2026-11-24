# Probar RaindropCmdPal en una laptop prestada

Para probar la extensión en una PC con **Windows 11 y admin**, sin instalar herramientas de desarrollo ni activar el
modo desarrollador. Toma unos 30 minutos y ~1 GB, que se recuperan al limpiar.

## Qué llevar (una carpeta)

| Archivo | De dónde sale |
|---|---|
| `RaindropCmdPal.cer` | Certificado de prueba **público**. Vence el 2027-11-23. |
| `RaindropCmdPal_x64.msix` (o `_arm64`) | Extensión firmada (~53 MB) |
| `instalar.ps1`, `limpiar.ps1` | Esta carpeta `scripts/` |

Los `.msix` y el `.cer` se generan en la máquina de build y **no van en git**. La clave privada (`.pfx`) nunca sale
de esa máquina.

## Pasos

1. Abre **PowerShell como administrador** en la carpeta y ejecuta:
   ```powershell
   powershell -ExecutionPolicy Bypass -File .\instalar.ps1
   ```
   El script instala PowerToys solo si falta Command Palette, confía en el certificado e instala la extensión.
2. Abre Command Palette (`Win+Alt+Space`) y ejecuta **Reload**.
3. Ve a **Search Raindrop** → ajustes de la extensión → pega tu token.
4. Prueba:
   - [ ] Al escribir aparecen resultados, sin trabarse.
   - [ ] Búsqueda por tag, por ejemplo `#IA`.
   - [ ] `Enter` abre el link en el navegador.
   - [ ] Menú contextual: **Copy link** y **Open in Raindrop**.
   - [ ] Íconos: las portadas de los bookmarks y la gota azul de la extensión.
   - [ ] Un token inválido muestra "Invalid API token".
   - [ ] Cambiar la duración de la caché y usar **Clear cache**.
5. Al terminar, en PowerShell como admin:
   ```powershell
   powershell -ExecutionPolicy Bypass -File .\limpiar.ps1
   ```
   Deshace solo lo que hizo `instalar.ps1`: quita la extensión y sus ajustes (que guardan el token), el certificado y
   PowerToys si lo instalamos nosotros.
6. **Regenera el token** en https://app.raindrop.io/settings/integrations, porque estuvo guardado en texto plano en
   una PC ajena.

## Si algo falla

- *"Windows 11 is required"*: Command Palette no existe en Windows 10. Hace falta otra laptop.
- `Add-AppxPackage` dice que el certificado no es de confianza: el paso 2 de `instalar.ps1` no se ejecutó como admin.
- La extensión no aparece en la paleta: ejecuta **Reload**, o cierra y vuelve a abrir Command Palette o PowerToys.
