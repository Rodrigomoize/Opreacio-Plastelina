# 🔧 TROUBLESHOOTING: Shake y Parpadeo no funcionan

## 🎯 Pasos para diagnosticar el problema:

### TEST 1: Verificar Shake de Barra de Intelecto

1. **En Unity Editor:**
   - Localizar el objeto que tiene el componente `IntelectBar` (probablemente en la UI del Canvas)
   - Añadir el componente `TestIntelectBarShake` a ese mismo objeto
   - Entrar en Play Mode
   - Presionar **ESPACIO**
   
2. **Resultados esperados en Console:**
   ```
   [TestIntelectBarShake] Presiona ESPACIO para probar el shake
   [TestIntelectBarShake] ESPACIO presionado - ejecutando shake
   [IntelectBar] ShakeBar llamado - duration=0.5, magnitude=15, activeInHierarchy=True
   ```

3. **Diagnóstico:**
   - ✅ Si aparecen los logs Y ves el shake visual → **FUNCIONA**
   - ⚠️ Si aparecen los logs pero NO ves shake → El RectTransform está mal configurado o el objeto padre está bloqueando el movimiento
   - ❌ Si NO aparecen los logs → El componente no está en el objeto correcto

---

### TEST 2: Verificar Parpadeo de Zonas

1. **En Unity Editor:**
   - Localizar el objeto que tiene el componente `DeploymentZoneFeedback`
   - Añadir el componente `TestDeploymentZoneFeedback` a ese mismo objeto
   - Entrar en Play Mode
   - Presionar **Z** para mostrar zonas

2. **Resultados esperados en Console:**
   ```
   [TestDeploymentZoneFeedback] Presiona Z para mostrar/ocultar zonas con parpadeo
   [TestDeploymentZoneFeedback] Z presionado - mostrando zonas
   [DeploymentZoneFeedback] ShowZones llamado - enablePulse=true
   [DeploymentZoneFeedback] Left zone enabled, color=(R:0.20, G:1.00, B:0.20, A:0.30)
   [DeploymentZoneFeedback] Right zone enabled, color=(R:0.20, G:1.00, B:0.20, A:0.30)
   [DeploymentZoneFeedback] PulseZones iniciado
   ```

3. **Diagnóstico:**
   - ✅ Si aparecen los logs Y ves el parpadeo → **FUNCIONA**
   - ⚠️ Si aparecen los logs pero NO ves parpadeo → Verificar:
     - Las imágenes `leftZoneImage` y `rightZoneImage` están asignadas en Inspector
     - Las imágenes tienen alpha > 0 en su color
     - El Canvas está en modo Screen Space
   - ❌ Si NO aparecen los logs → El componente no está en el objeto correcto

---

### TEST 3: Verificar que se llama desde el juego real

1. **Entrar en Play Mode** y jugar normalmente
2. **Intentar tirar una carta SIN suficiente intelecto**

3. **Resultados esperados en Console:**
   ```
   [PlayerCardManager] Intelecto insuficiente detectado - carta simple
   [IntelectBar] ShakeBar llamado - duration=0.5, magnitude=15, activeInHierarchy=True
   ```

4. **Diagnóstico:**
   - ✅ Si aparecen ambos logs → El sistema está conectado correctamente
   - ⚠️ Si solo aparece el primer log → `intelectBar` NO está asignado en `PlayerCardManager`
   - ❌ Si NO aparece ningún log → El sistema de intelecto no está detectando correctamente la falta de recursos

---

### TEST 4: Verificar parpadeo al seleccionar cartas

1. **Entrar en Play Mode**
2. **Seleccionar una carta** (hacer click en una carta de tu mano)

3. **Resultados esperados en Console:**
   ```
   [DeploymentZoneFeedback] ShowZones llamado - enablePulse=true
   [DeploymentZoneFeedback] Left zone enabled, color=...
   [DeploymentZoneFeedback] Right zone enabled, color=...
   [DeploymentZoneFeedback] PulseZones iniciado
   ```

4. **Diagnóstico:**
   - ✅ Si aparecen los logs → `PlayableAreaUI` está llamando correctamente a `DeploymentZoneFeedback`
   - ❌ Si NO aparecen → Verificar que `zoneFeedback` está asignado en `PlayableAreaUI`

---

## 🛠️ SOLUCIONES COMUNES:

### Problema: "Shake no se ve visualmente"
**Solución 1:** El objeto padre puede estar bloqueando el movimiento
- Verificar que el objeto con `IntelectBar` no tenga `Layout Group` en sus padres
- Verificar que no tenga un `Content Size Fitter` que resetee la posición

**Solución 2:** El RectTransform está anclado de forma que el movimiento no se nota
- Cambiar anchors a center/center
- Probar con `magnitude = 50f` para hacer el shake más evidente

### Problema: "Parpadeo no se ve"
**Solución 1:** Las imágenes no están asignadas
- En Inspector, verificar que `leftZoneImage` y `rightZoneImage` tienen referencias

**Solución 2:** enablePulse está desactivado
- En Inspector del `DeploymentZoneFeedback`, asegurar que `Enable Pulse = true`

**Solución 3:** Las imágenes están ocultas por otras capas
- Verificar el orden de las capas en el Canvas
- Verificar que las imágenes no tengan alpha = 0 permanentemente

**Solución 4:** El color base es demasiado transparente
- En Inspector, cambiar `availableColor` a algo más visible temporalmente:
  - R: 0.2, G: 1.0, B: 0.2, **A: 0.8** (en vez de 0.3)

### Problema: "intelectBar es null"
**Solución:**
- Abrir escena de combate
- Seleccionar el GameObject que tiene `PlayerCardManager`
- En Inspector, arrastrar el objeto con `IntelectBar` al campo `Intelect Bar`

### Problema: "zoneFeedback es null"
**Solución:**
- Abrir escena de combate
- Seleccionar el GameObject que tiene `PlayableAreaUI`
- En Inspector, arrastrar el objeto con `DeploymentZoneFeedback` al campo `Zone Feedback`

---

## 📊 CHECKLIST DE VERIFICACIÓN:

### Barra de Intelecto - Shake:
- [ ] Objeto tiene componente `IntelectBar`
- [ ] Objeto está activo en jerarquía
- [ ] `PlayerCardManager` tiene referencia a `intelectBar` asignada
- [ ] Presionar ESPACIO con `TestIntelectBarShake` muestra el shake
- [ ] No hay Layout Groups bloqueando el movimiento

### Zonas de Deployment - Parpadeo:
- [ ] Objeto tiene componente `DeploymentZoneFeedback`
- [ ] `leftZoneImage` está asignado en Inspector
- [ ] `rightZoneImage` está asignado en Inspector
- [ ] `enablePulse = true` en Inspector
- [ ] `pulseSpeed = 1.5` y `pulseIntensity = 0.3` (o valores mayores para testing)
- [ ] `PlayableAreaUI` tiene referencia a `zoneFeedback` asignada
- [ ] Presionar Z con `TestDeploymentZoneFeedback` muestra el parpadeo

---

## 🎬 PRÓXIMOS PASOS:

1. Ejecutar TEST 1 y TEST 2 con los scripts de prueba
2. Revisar Console para ver exactamente qué logs aparecen
3. Basándose en los resultados, aplicar las soluciones correspondientes
4. Si todo funciona en los tests pero no en el juego, verificar las asignaciones de referencias

---

## 📝 NOTA IMPORTANTE:

Los logs de debug (LogWarning) están activos y deberían aparecer en la consola de Unity cuando ejecutes los tests. Si NO ves NINGÚN log, significa que:
- El código no se está ejecutando
- El objeto está desactivado
- El componente no está en el objeto que crees

**Revisar la consola PRIMERO antes de asumir que algo no funciona.**
