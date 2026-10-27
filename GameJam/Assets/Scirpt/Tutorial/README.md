# Tutorial และเสียงที่ย้ายมาใช้กับ Space Garage

อัปเดต 2026-10-27

ตัด dependency ของโปรเจกต์เก่าออก: TutorialDialog/TutorialClick ใช้ข้อความ ENG และ font ที่ผู้ใช้ผูกไว้ ไม่เรียก GlobalQuestState หรือ InteractableObject ไม่เปลี่ยน panel/animation เดิม TutorialSignal เก็บเงื่อนไข chapter เดิมไว้ แต่ใช้ Current Chapter ของตัวเอง (เริ่ม 0) และ public SetChapter(int) แทนระบบ quest ที่ไม่มีในเกมนี้ ถ้าไม่ใช้ chapter ให้ปิด Raise On Chapter; trigger แบบ Start/Enable/UnityEvent ยังทำงานเดิม

UISfxManager ไม่ต้องมี GameSfx หรือ UIButtonEffect อีกต่อไป เก็บชื่อช่องเสียง UI/extra clips/เพลงเดิมและ GUID สคริปต์ไว้ ไม่มีระบบ Setting เก่าควบคุม volume แล้ว ใช้ Volume สำหรับ UI, Music Volume สำหรับเพลง และ Gameplay Volume คูณ Weapon Volume/Jet Volume สำหรับเสียงยาน

เพิ่มช่อง AudioClip: Projectile Shot, Rail Shot, Plasma Shot, Getsuka Shot, Laser Loop, Normal Jet Loop, Boost Jet Loop ช่องว่างจะเงียบ รองรับเสียงสามมิติผ่าน Gameplay Spatial Blend, Gameplay Min Distance และ Gameplay Max Distance เสียงยิงหนึ่งครั้งต่อชุดยิง; Laser วนขณะยิง; ไอพ่นสลับ normal/boost ไม่เล่นซ้อน เสียงยานหยุดตามเจ้าของและ pause เมื่อ Time.timeScale เป็น 0 เสียง UI ยังเล่นได้

ต้องใส่ UISfxManager บน GameObject ในซีนเริ่มและลาก AudioClip เอง เปิด Dont Destroy On Load เพื่ออยู่ข้ามซีน ถ้าเริ่มทดสอบจากซีนอื่นโดยตรงต้องมี manager ในนั้นด้วย ไม่สร้าง Canvas หรือเปลี่ยน layout ใด ๆ Auto Bind Scene Buttons ผูก click และเพิ่ม UISfxButtonHover สำหรับ hover; ปุ่มที่สร้างภายหลังเรียก BindButtons(root) หลังสร้าง (CustomizationUI เชื่อมแล้ว) ปุ่มที่มี UIButtonEffect เดิมจะไม่ผูกซ้ำ

ShipGun เชื่อมเสียงอาวุธ, ShipController/Enemy เชื่อมไอพ่น และ CustomerRepairShip เชื่อมขณะบินเข้าออก ไม่เล่นจาก ShipBoostEffect แต่ละชิ้นเพื่อหลีกเลี่ยงเสียงซ้ำและ preview มีเสียง

ผลตรวจ: คอมไพล์ Assembly-CSharp ด้วย Roslyn และ response file ของ Unity รวม Tutorial/SpaceGarage ผ่าน ไม่มี error ยังไม่ได้ทดสอบฟังเสียงหรือ Play Mode ใน Editor ต้อง Refresh และใส่คลิปก่อนตรวจเสียงจริง
