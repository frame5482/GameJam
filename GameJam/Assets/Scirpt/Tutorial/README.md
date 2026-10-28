# Tutorial และเสียงที่ย้ายมาใช้กับ Space Garage

อัปเดต 2026-10-27

ตัด dependency ของโปรเจกต์เก่าออก: TutorialDialog/TutorialClick ใช้ข้อความ ENG และ font ที่ผู้ใช้ผูกไว้ ไม่เรียก GlobalQuestState หรือ InteractableObject ไม่เปลี่ยน panel/animation เดิม TutorialSignal เก็บเงื่อนไข chapter เดิมไว้ แต่ใช้ Current Chapter ของตัวเอง (เริ่ม 0) และ public SetChapter(int) แทนระบบ quest ที่ไม่มีในเกมนี้ ถ้าไม่ใช้ chapter ให้ปิด Raise On Chapter; trigger แบบ Start/Enable/UnityEvent ยังทำงานเดิม

UISfxManager ไม่ต้องมี GameSfx หรือ UIButtonEffect อีกต่อไป เก็บชื่อช่องเสียง UI/extra clips/เพลงเดิมและ GUID สคริปต์ไว้ ไม่มีระบบ Setting เก่าควบคุม volume แล้ว ใช้ Volume สำหรับ UI, Music Volume สำหรับเพลง และ Gameplay Volume คูณ Weapon Volume/Jet Volume สำหรับเสียงยาน

เพิ่มช่อง AudioClip: Projectile Shot, Rail Shot, Plasma Shot, Getsuka Shot, Laser Loop, Normal Jet Loop, Boost Jet Loop ช่องว่างจะเงียบ รองรับเสียงสามมิติผ่าน Gameplay Spatial Blend, Gameplay Min Distance และ Gameplay Max Distance เสียงยิงหนึ่งครั้งต่อชุดยิง; Laser วนขณะยิง; ไอพ่นสลับ normal/boost ไม่เล่นซ้อน เสียงยานหยุดตามเจ้าของและ pause เมื่อ Time.timeScale เป็น 0 เสียง UI ยังเล่นได้

ต้องใส่ UISfxManager บน GameObject ในซีนเริ่มและลาก AudioClip เอง เปิด Dont Destroy On Load เพื่ออยู่ข้ามซีน ถ้าเริ่มทดสอบจากซีนอื่นโดยตรงต้องมี manager ในนั้นด้วย ไม่สร้าง Canvas หรือเปลี่ยน layout ใด ๆ Auto Bind Scene Buttons ผูก click และเพิ่ม UISfxButtonHover สำหรับ hover; ปุ่มที่สร้างภายหลังเรียก BindButtons(root) หลังสร้าง (CustomizationUI เชื่อมแล้ว) ปุ่มที่มี UIButtonEffect เดิมจะไม่ผูกซ้ำ

ShipGun เชื่อมเสียงอาวุธ, ShipController/Enemy เชื่อมไอพ่น และ CustomerRepairShip เชื่อมขณะบินเข้าออก ไม่เล่นจาก ShipBoostEffect แต่ละชิ้นเพื่อหลีกเลี่ยงเสียงซ้ำและ preview มีเสียง

ผลตรวจ: คอมไพล์ Assembly-CSharp ด้วย Roslyn และ response file ของ Unity รวม Tutorial/SpaceGarage ผ่าน ไม่มี error ยังไม่ได้ทดสอบฟังเสียงหรือ Play Mode ใน Editor ต้อง Refresh และใส่คลิปก่อนตรวจเสียงจริง

Settings (2026-10-27): UISfxManager อ่าน GarageSettings.MusicVolume/SfxVolume เพิ่มเป็นตัวคูณหน้า Settings ใหม่ ทั้ง UI/one-shot/loop ที่กำลังเล่น Master ผ่าน AudioListener.volume บันทึก PlayerPrefs ไม่ใช้ GameSfx ไม่เปลี่ยนคลิปหรือ gain เดิม ช่อง Slider อยู่ SpaceGarage/UI/README.md

## บทสอน Repair ไป Explore ครั้งเดียว (2026-10-27)

ผู้ใช้อนุญาตให้ทำกล่องง่าย ๆ จึงเพิ่ม UGUI prefab `SpaceGarage/UI/FirstRepairTutorialDialog.prefab` กล่องพื้นเข้มด้านบนกลาง 760x170 มี Title/Instructions/Step Counter ใช้ฟอนต์ TMP เดิมใน Repair ไม่ใช้ UI Toolkit ไม่มี Next/Skip, dim, highlight, เฟดหรือแอนิเมชันใหม่ ใช้ TutorialDialog เดิม เปิดปิด root ด้วย Show/Hide ไม่จัดตำแหน่ง UI เดิม ไม่หยุดเวลา

เพิ่ม root `FirstRepairTutorial` ใน Scenes/Repair.unity มี TutorialManager เดิมที่กรอก Steps จริง 5 ข้อและ GarageRepairTutorial bridge:

1. ClickButton: Repair ปุ่มเดิม 271764655 — ชวนเปิดงานซ่อม
2. WaitSignal: Garage.FirstRepair.Docked — รอ CustomerRepairState.Docked จริง
3. WaitSignal: Garage.FirstRepair.OnePartRepaired — อธิบายให้ลากวัสดุที่ตรงไปยังชิ้นส่วน ตรวจอย่างน้อยหนึ่ง RepairableShipPart ที่ OriginalDamagePercent>0 และ IsRepaired จริง ไม่ใช่เพียงเริ่มลาก/ปล่อยผิดชิ้น และไม่ต้องซ่อมทั้งลำ
4. ClickButton: Back เดิม 705761708 — กลับ Main UI โดยไม่ต้องกดส่งยาน
5. ClickButton: Explore เดิม 1673099990 — เริ่มสำรวจต่อสู้ จบบทและบันทึก PlayerPrefs ก่อนเปลี่ยนซีน

GarageRepairTutorial รับ Tutorial/Workshop/Main Menu Panel/Start Panel ผูกแล้ว ตรวจ Main Menu เปิดจริงและ Start ปิดก่อนส่ง Garage.FirstRepair.Begin จึงไม่เริ่มทับหน้าชื่อเกม ฟัง workshop.Changed เพื่อส่งสัญญาณเมื่อจอด/ซ่อมสำเร็จ และถอด listener ตอนปิด ไม่เปลี่ยนการสุ่มสูตร จำนวนของ ดรอป หรือพฤติกรรมซ่อม/พักงานเดิม

TutorialManager ตั้ง tutorialKey=SpaceGarage.FirstRepair, playOnce=true, startSignalKey=Garage.FirstRepair.Begin, startDelay=0.4, pauseGameWhileRunning=false, Dialog Prefab/Parent Canvas เดิมผูกแล้ว เมื่อจบบทใช้ PlayerPrefs `Tutorial_Done_SpaceGarage.FirstRepair=1` จาก MarkDone เดิมพร้อม Save จึงไม่ขึ้นอีกเมื่อกลับจากต่อสู้หรือปิดเปิดเกม ไม่ล้างสถานะผู้ใช้ระหว่างพัฒนา ถ้ายังเรียนไม่จบและออกเกม จะเริ่มบทอีกครั้งจนจบ

ผู้ใช้แก้ข้อความอังกฤษ/ปุ่ม/จำนวนขั้นที่ TutorialManager > Steps บน FirstRepairTutorial และแก้สี/ขนาดกล่องได้ที่ prefab โดยตรง ไม่มี reference ที่ต้องลากเพิ่ม หากเปิดซีนเก่าค้างต้องโหลด Repair ใหม่หลังเซฟงานเพื่อรับวัตถุที่เพิ่มในไฟล์

ตรวจข้อมูล: scene/prefab fileID ไม่ซ้ำ, SceneRoots มี tutorial, ปุ่มจริงสามปุ่มและ Dialog GUID ตรง, Steps 5/Play Once/เวลาเดินครบ คอมไพล์ Assembly-CSharp ทั้งชุดด้วย Roslyn/reference Unity ผ่าน exit 0 ไม่ได้ Build Player และยังไม่ได้เรนเดอร์กล่องหรือเล่นบทสอนจริงใน Play Mode ไม่มีไฟล์ตรวจชั่วคราวค้าง
