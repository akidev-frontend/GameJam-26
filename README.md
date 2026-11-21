# 🕹️ Reglas Rotas — Game Jam 26

> Juego de **plataforma 2D estilo pixel art** donde **cada nivel cambia las reglas del juego**.
> Tu único objetivo en cada nivel es llegar hasta el objeto y conseguirlo… si las nuevas reglas te lo permiten.

---

## 🎯 Idea central

Un plataformeo básico (o no tanto) para llegar a un objeto. Lo que rompe el juego es que **las reglas mutan en cada nivel**: los controles se invierten, la gravedad se pone patas arriba, o incluso tienes que **morir** para pasar… aunque nada te mate.

| Aspecto | Descripción |
|---|---|
| **Género** | Plataforma 2D |
| **Estilo visual** | Pixel art |
| **Cámara** | Lateral fija / con ligero seguimiento |
| **Bucle de juego** | Entrar en nivel → descubrir la regla → adaptarte → conseguir el objeto → siguiente nivel |
| **Objetivo** | Conseguir el objeto que aparece en pantalla para desbloquear la siguiente fase |

---

## 🎮 Cómo se juega

1. **Entras en un nivel.** Todo parece un plataformeo normal.
2. **Descubres la regla.** Algo cambia: los controles, la gravedad, la muerte…
3. **Te adaptas.** El level design está pensado para que la regla sea el verdadero obstáculo.
4. **Consigues el objeto.** Al tocarlo, el nivel se supera y pasas al siguiente.

Los controles base (sin reglas extrañas):

| Acción | Teclado | Mando |
|---|---|---|
| Moverse | `A` / `D` o `←` / `→` | Stick izquierdo |
| Saltar | `Espacio` / `W` | A |
| Reiniciar nivel (si te atascas) | `R` | Select / Back |

*La UI recuerda la regla del nivel en todo momento (rótulo al entrar + indicador persistente).*

---

## 📋 Los niveles

### Nivel 1 — Tutorial
- **Regla:** *Ninguna.* Controles estándar, gravedad normal.
- **Objetivo:** Aprender a moverte y saltar hasta el objeto.
- **Detalle:** Aquí el juego te miente: parece un plataformeo tranquilo.

### Nivel 2 — Controles cambiados
- **Regla:** Los controles se alteran (izquierda/derecha invertidos, o el salto ligado a otra tecla).
- **Objetivo:** Llegar al objeto luchando contra tus propios reflejos.
- **Detalle:** El level design es sencillo a propósito: el desafío es mental, no físico.

### Nivel 3 — Gravedad invertida
- **Regla:** La gravedad apunta hacia arriba. Saltas "hacia abajo" y caminas por el techo.
- **Objetivo:** Releer el escenario completo: el techo es el suelo.
- **Detalle:** Plataformas que antes eran suelo ahora son techos.

### Nivel 4 — Morir para ganar
- **Regla:** Necesitas **morir** para completar el nivel… pero **nada te mata**.
- **Objetivo:** Encontrar la forma de provocar tu propia muerte (caída al vacío, trampas del escenario, un mecanismo secreto) para "conseguir" el paso de nivel.
- **Detalle:** El objeto puede aparecer donde nunca llegarías con vida. El juego invierte la lógica: **la muerte es la victoria**.

### Nivel 5 — (Por definir)
- **Regla:** *TBD* — candidatas:
  - El personaje solo se mueve cuando **no** pulsas nada.
  - El objeto **huye** de ti.
  - Todo es **invisible** salvo un radio alrededor del jugador.
  - La cámara gira / el nivel rota.
- **Objetivo:** Mantener la sorpresa y cerrar el juego con una regla que rompa el rompe-reglas.

---

## 🧠 Pilares de diseño

1. **Lectura inmediata:** la regla de cada nivel debe entenderse en menos de 5 segundos.
2. **Regla = obstáculo:** el level design no compite con la regla, la *es*.
3. **Frustración justa:** morir o fallar debe sentirse culpa del jugador, no del juego.
4. **Sorpresa escalonada:** cada nivel debe superar la rareza del anterior.
5. **Consistencia dentro del caos:** los controles base y el HUD nunca cambian, solo la regla.

---

## 🛠️ Aspecto técnico

| Elemento | Detalle |
|---|---|
| **Motor** | Unity 6000.3 (URP 2D) |
| **Input** | Input System (nuevo) |
| **Estilo** | Pixel art, resolución nativa baja escalada con filtro *Point* |
| **Física** | Rigidbody2D + Colliders 2D |
| **Arquitectura** | Cada regla = un componente/ScriptableObject desactivable por nivel |
| **Sistema de reglas** | `RuleManager` aplica/desactiva modificadores al cargar cada nivel |

### Estructura prevista de scripts

```
Assets/Scripts/
├── Player/          # Movimiento, salto, input
├── Rules/           # Reglas por nivel (gravedad, controles, muerte…)
├── Level/           # Carga de nivel, objeto final, checkpoints
├── UI/              # Rótulo de regla, HUD, pantalla de victoria
└── Core/            # RuleManager, GameManager
```

---

## 📌 Estado actual

- [x] Proyecto creado en Unity 6 (plantilla 2D)
- [x] MCP for Unity conectado
- [x] Repositorio en GitHub
- [ ] Movimiento del jugador (input + físicas 2D)
- [ ] Sistema de reglas por nivel
- [ ] Objeto final y transición entre niveles
- [ ] Niveles 1–4 jugables
- [ ] Pixel art y audio

---

*Game Jam 26 — ¡Las reglas cambian, tú también!*
