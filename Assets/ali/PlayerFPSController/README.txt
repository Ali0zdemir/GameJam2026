# 🎮 FPS Player Controller – Kullanım Rehberi


## ✅ 1. Player Kurulumu

### Player GameObject oluştur
Sahneye boş bir GameObject ekle ve adını ve tag ini `Player` yap.

Player objesine şunları ekle:
- `Rigidbody`
- `CapsuleCollider`
- `PlayerMovementA`
- `Health`

### Rigidbody Ayarları
- **Use Gravity:** Açık
- **Interpolation:** Interpolate
- **Freeze Rotation:**  
  - X ✔  
  - Y ✔  
  - Z ✔  


## 🎥 2. Kamera Kurulumu

### Kamera Oluştur
- Player objesinin altına bir **Camera** ekle
- Kameranın tagi MainCamera olmalı
- Kamerayı kafa hizasına getir

### Kamera Script Ayarları
- Kameraya `FirstPersonCamera` scriptini ekle
- `Player Body` alanına **Player objesini** sürükle
