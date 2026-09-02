# Animator Controller Guide for DestopDunge

นี่เป็นคู่มือสั้น ๆ สำหรับการตั้งค่า Animator Controller ที่ใช้ร่วมกับ CharacterData / TamagotchiBehaviour / CombatController

Parameters (แนะนำให้มี):
- bool IsWorking   --> อยู่ในโหมด Tamagotchi (Idle / Walk)
- bool IsDungeon   --> อยู่ในโหมด Dungeon (Combat)
- trigger DoTransition --> เล่นอนิเมชันแปลงร่าง
- float AttackSpeed --> ปรับความเร็วของ animator สำหรับคอมโบ
- int AttackIndex --> เลือกคอมโบโจมตี

States (Base Layer):
- Idle (IsWorking == true)
- Walk (blend/animation)
- Transition (trigger DoTransition)
- CombatIdle (IsDungeon == true)
- CombatMove
- Attack (use AttackIndex to branch/combo)

Transition tips:
- Transition จาก Idle -> Transition โดยใช้ DoTransition trigger
- Transition จาก Transition -> CombatIdle โดย HasExitTime = true และช่วงสั้น (0.2-0.6s)
- ใช้ Animator.speed หรือ parameter AttackSpeed เพื่อปรับจังหวะการโจมตีตาม CharacterData.attackSpeed

ประสิทธิภาพ:
- สำหรับ Desktop Tamagotchi ให้ animation ของ desktopPrefab ใช้ low-bone rigs หรือ sprite flipbook
- ลดจำนวน layers และ blend trees ถ้าไม่จำเป็น

การทดสอบ:
- ใน Editor ตั้งค่า DesktopCharacterController ให้เรียก animator parameter ตามสถานะ (IsWorking/IsDungeon)
- ทดสอบ Transition ด้วยการกด ToggleMode เพื่อดูว่าการเปลี่ยน animator เป็นไปอย่างราบรื่น

