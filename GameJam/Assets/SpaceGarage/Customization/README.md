# Ship Customization and JSON Save

## ทดลองชิ้นส่วนตอนกดเลือก (2026-10-27)

แก้ `Scripts/ShipCustomization.cs`: เมื่อคลิกรายการในหน้าปรับแต่ง ยาน CustomizationPreview เดิมเปลี่ยนเฉพาะช่องที่เลือกเป็น Part Prefab ของชิ้นนั้นทันที ทดลองได้ทั้งชิ้นที่ปลดแล้วและยังล็อก อีกสามช่องใช้ชุด Equipped ปัจจุบัน ยังคงไฮไลต์/ลอยของหมวดที่เลือกด้วย CustomizationPreviewPart เดิม ไม่เพิ่มแอนิเมชันหรือเปลี่ยน panel/layout/กล้อง

เลือกอย่างเดียวไม่ซื้อ ไม่หักเงิน ไม่ใส่จริง และไม่เขียนชุดทดลองลงเซฟ ต้องกด Unlock แล้ว Equip ตามเดิม การอัปเดตเงิน/ปลดล็อกยังคงชิ้นที่กำลังทดลองไว้ เมื่อเปลี่ยนหมวดจะคืนช่องเดิมเป็นชิ้น Equipped และเริ่มเลือกชิ้น Equipped ของหมวดใหม่ เมื่อปิดหน้าปรับแต่งยานหน้าหลักกลับไปใช้ชุด Equipped จริงทั้งหมด

ไม่มีช่อง Inspector ใหม่ ไม่แก้ scene/prefab และไม่ต้องตั้งค่าเพิ่ม ผลตรวจ: คอมไพล์ runtime ทั้งชุดด้วย Roslyn/references Unity จริงผ่าน ตรวจเส้นทาง SelectPart/SelectCategory/RefreshPreview/CloseCustomization และ diff แล้ว ยังไม่ได้คลิก UI/ตรวจภาพการเปลี่ยนโมเดลจริงใน Play Mode

## ราคาปลดล็อกและชุดเริ่มต้นล่าสุด (2026-10-27)

อัปเดตตามผู้ใช้ล่าสุด แทนรายการฟรีทั้งหมดในบันทึกก่อนหน้า: CustomizationCatalog.asset มี Initially Unlocked เฉพาะ Base Gun / Base Plate / Base Wing / Base Jet อย่างละหนึ่งชิ้น ส่วนที่เหลืออยู่ Locked หมวดละ 4 ชิ้น รวม 16 ชิ้น ใช้ระบบซื้อแล้วกด Equip แยกเดิม

| ชุด | Gun | Plate | Wing | Jet |
| --- | ---: | ---: | ---: | ---: |
| Base | ฟรี | ฟรี | ฟรี | ฟรี |
| Heavy | Heavy Gun 900 | Heavy Plate 900 | Tank Wing 850 | Burst Jet 1000 |
| Light | Light Gun 800 | Light Plate 800 | Swept Wing 900 | Cruise Jet 900 |
| Barrier | Barrier Gun 1100 | Barrier Plate 1500 | Agile Wing 1000 | Twin Jet 1400 |
| Unity | Unity Gun 1200 | Unity Plate 1800 | Unity Wing 1800 | Overdrive 1600 |

เปลี่ยนเฉพาะ Display Name ของปืน Light/Barrier/Unity ให้แยกชื่อในร้าน ไม่เปลี่ยนความสามารถ/Part ID/GUID ปืน ราคาเป็นเงินต่อการปลดล็อกถาวร ปรับได้ที่ Pools > Locked > Price บน CustomizationCatalog ไม่มีช่อง Inspector ใหม่ ไม่แก้ UI หรือวิธีเปิด/ปิดแผง

ไม่มีเซฟหรือกด New Game จะเริ่ม Base ครบสี่ส่วน เซฟเก่ายังรักษารายการที่ซื้อไว้จริง ไม่ล้าง JSON/เงิน/คลังเพื่อทดสอบ การปลดฟรีจาก catalog เก่าไม่ได้ถือเป็นการซื้อ จึงจะล็อกตาม catalog ใหม่หลังโหลดและเปลี่ยนชุดที่ไม่มีสิทธิ์กลับไปชิ้นที่ปลดแล้วตาม ResourceManager เดิม

ผลตรวจ: ตรวจ catalog ครบ 20 ชิ้น Slot/ID/reference ตรงหมวด ฟรีเฉพาะ Base 4 ชิ้นและ Locked ราคาบวก 16 ชิ้น พูลศัตรู/โรงซ่อมหมวดละ 5 ตรงกับ catalog ตรวจข้อมูลและ diff เฉพาะงานนี้ ไม่ได้ทดสอบซื้อ/สุ่มใน Play Mode ไม่มีการเปลี่ยน runtime code จึงไม่ได้คอมไพล์ซ้ำ ไม่ต้องตั้งค่าเพิ่ม เริ่ม Play ใหม่เพื่อโหลด asset/scene ที่แก้

อัปเดต 2026-10-27: เปลี่ยนชื่อ Plate/Wing/Jet ตามประเภทครบ 15 ชิ้น เพิ่ม Light/Unity หมวดเหล่านี้ใน Initially Unlocked ตามนโยบายฟรีเดิม คง GUID/Part ID ของ Starter/Heavy/Barrier และให้ ID ไม่ซ้ำเฉพาะสำเนาใหม่ Light/Unity ที่ยังไม่อยู่ใน catalog ไม่แก้ข้อมูล JSON ผู้ใช้ ไม่เซฟ Unity mode/โล่/charge ชั่วคราว รายละเอียดชื่อไฟล์และช่องใหม่อยู่ [Examples/README.md](../Examples/README.md) ไม่เพิ่มช่องหรือเปลี่ยน layout/วิธีเปิดปิด UI

อัปเดต 2026-10-26 ใช้ Canvas ของผู้ใช้ ข้อความบน UI เป็น ENG ไม่สร้างหน้าตา Panel หรือจัด layout แทน

## แก้รายการไม่ขึ้นและ Initially Unlocked ยังล็อก (2026-10-26)

พบ Gun pool ใส่ HeavyArmorPlate แทน HeavyGun ทำให้ถูกกรองเพราะ Slot ไม่ตรง และชุด Barrier ที่คัดลอกมาทั้งสี่ชิ้นมี Part ID ซ้ำกับ Heavy ทำให้ Catalog.Find คืน null เพื่อป้องกันเซฟอ้างผิดชิ้น ส่งผลให้ IsUnlocked เป็น false และรายการถูกตัดซ้ำตาม ID

แก้ CustomizationCatalog.asset ให้ช่องปืนอ้าง HeavyGun และแก้เฉพาะ Part ID ของ asset Barrier ชุดใหม่ BarrieGun/BarrieArmorPlate/BarrieWing/BarrieJet ให้ใช้ ID เฉพาะของแต่ละ asset (ตรงกับ GUID ของ asset นั้น) เก็บ Part ID ของ Starter/Heavy เดิมทั้งหมด ไม่ล้างหรือเขียนย้ายเซฟเก่า ไม่แก้สคริปต์/ซีน/UI/prefab และไม่มีช่อง Inspector ใหม่ Initially Unlocked ตรวจจาก catalog ปัจจุบันแม้มีเซฟแล้ว จึงไม่ต้องลบเซฟเพื่อให้ปลดล็อก

ตรวจข้อมูลอ้างอิงใน catalog ครบ 12 ชิ้น (หมวดละ 3) พบ asset ครบ Slot ตรงหมวดและ Part ID ไม่ซ้ำ ตรวจ git diff --check ผ่าน ยังไม่ได้กด UI หรือทดสอบ Equip/ภาพยานใน Unity Play Mode ผู้ใช้ไม่ต้องผูก reference เพิ่ม; หากกำลัง Play ให้ออกแล้วเข้า Play ใหม่เพื่อโหลดข้อมูลที่แก้
## ตั้งค่าพูลทั้งสี่หมวด

แก้ Jet ต่อไม่ถึงตัวถัง: JetSpawnPoint ของ CustomizationPreview ใน Repair.unity และ prefab CustomerRepairShip/EnemyShip/StarterShip ทั้งสองชุดใช้ local (0,0,0) แทน z=-1.05 เพราะ mesh Jet และ ArmorPlate จาก Ship1.fbx ใช้ตำแหน่งประกอบในข้อมูลโมเดลอยู่แล้ว SampleScene มี Jet z=0 override เดิมอยู่แล้ว ไม่ขยับโมเดล/ไอพ่นหรือเปลี่ยน Material ไม่มีช่องใหม่ ตรวจค่าจุดติดตั้งแล้ว ยังไม่ได้ตรวจภาพ Play Mode หลังแก้

เปิด `CustomizationCatalog.asset` มี Pools แยก Gun / ArmorPlate / Wing / Jet แต่ละหมวดมี Initially Unlocked (ใส่ฟรีตั้งแต่เริ่ม) และ Locked (Part กับ Price สำหรับปลดล็อกด้วยเงิน) ใส่ ShipPartStats ที่ Slot ตรงหมวด และมี Part Prefab ตัวอย่าง Initially Unlocked ใส่ StarterShip ทั้งสี่ส่วนแล้ว Locked ยังว่างให้เพิ่มชิ้นส่วน/ราคาเอง ไม่สร้างของขายสมมุติ

ShipPartStats เพิ่ม Part ID, Display Name และ Icon: Part ID ต้องไม่ซ้ำและคงเดิมเพื่อใช้กับเซฟ ระบบกำหนดให้ข้อมูลเดิมแล้ว และสร้าง ID เมื่อสร้าง asset ใหม่ ถ้าทำ Duplicate asset ให้เปลี่ยน Part ID ของสำเนาเป็นค่าใหม่ก่อนเพิ่มใน catalog; Display Name ใช้ชื่อ ENG; Icon รับ Sprite รูปที่แสดงในรายการ ไม่ใส่ชื่อจะใช้ชื่อ asset

ResourceManager.prefab ผูก Customization Catalog แล้ว ถ้าเปลี่ยน catalog ให้เปลี่ยนบน prefab กลางเพื่อให้หน้าโรงซ่อมและฉากต่อสู้เห็นข้อมูลชุดเดียวกัน

## ต่อหน้า Canvas

แก้การสลับกล้อง: เปิด GameObject ของกล้องที่เลือกด้วย (รองรับกล้องที่ปิดไว้ใน Hierarchy) แล้วเปิดเฉพาะ CinemachineCamera ของหน้านั้น ไม่เปลี่ยนตำแหน่ง/มุมที่ผู้ใช้จัดไว้ หาก reference ว่างจะค้น MainMenuCamera / CinemachineCamera / CustomizationCamera รวมตัวที่ inactive และค้น ShipCustomization ตอนเริ่ม มีการผูกช่องว่างใน Repair.unity ใหม่แล้ว

หน้า Main UI แสดงยานผู้เล่นจอดจากชุด Equipped ปัจจุบันโดยใช้ CustomizationPreview เดิมร่วมกับหน้าปรับแต่ง ยานหันตามแนวตำแหน่งจอดไป Out เมื่อกด Repair ยานบินไป Out ก่อนซ่อน เมื่อออก Repair ยานเกิดที่ In แล้วบินกลับจุดจอดเดิม หากกดกลับกลางทางจะบินกลับจากตำแหน่งปัจจุบันโดยไม่วาร์ปกลับ In ไอพ่นปกติเปิดระหว่างบิน ดับเมื่อจอด ไม่มีการยิงหรือบูสต์ ปรับความเร็วได้ที่ ShipCustomization > Docking Flight Speed (เริ่มต้น 25) ตำแหน่งจอดใช้ตำแหน่ง CustomizationPreview ตอนเริ่มเกม ไม่ย้ายจุด In/Stay/Out

ปุ่มออก Repair: ลาก Button ของคุณลง RepairSystems > RepairUI > Repair Back Button ระบบผูก onClick ให้ หรือให้ On Click เรียก RepairUI.CloseRepair / CustomerRepairManager.CloseRepair เองโดยไม่ผูกซ้ำ ต้องเรียกเมธอดนี้แทนแค่สลับ Panel เพื่อเปลี่ยนกล้องและให้ยานกลับมา งานซ่อมลูกค้าปัจจุบันจะพักไว้ (ซ่อนยานลูกค้า) เปิด Repair อีกครั้งจะทำงานเดิมต่อโดยไม่สุ่มใหม่หรือหักวัสดุซ้ำ งานที่พักยังไม่ได้เซฟลง JSON เมื่อออกเกม/ออกซีน

เลือก RepairSystems ใน `Scenes/Repair.unity` มี ShipCustomization และ CustomizationUI เพิ่มแล้ว เก็บสคริปต์ไว้บน object ที่เปิดอยู่เสมอ ไม่ไว้ใต้แผงที่จะปิด

- ShipCustomization: Workshop ผูกระบบซ่อม; Preview Root ผูก CustomizationPreview ที่ตำแหน่ง Stay มี Gun / Armor Plate / Wing / Jet Mount ครบ ใช้จุดเหล่านี้ปรับตำแหน่งประกอบ ระบบสร้างเฉพาะชิ้นส่วนยาน ไม่บิน/ไม่ยิงขณะปรับแต่ง
- CustomizationUI > Customization: ผูก ShipCustomization แล้ว
- Customization Panel: GameObject แผงหน้าปรับแต่งของคุณ ระบบสลับด้วย SetActive
- Open Button / Back Button: Button เข้าและกลับเมนูหลัก ระบบผูก onClick ให้ ไม่ต้องเพิ่มซ้ำ
- Gun / Armor / Wing / Jet Category Button: Button เลือกหมวดทั้งสี่
- Part Container: RectTransform เดียวสำหรับรายการทั้งหมด จัด Grid/Vertical Layout เอง
- Part Row Prefab: prefab UI ของคุณที่ติด CustomizationPartRow ช่อง Select Button รับ Button กดเลือก, Icon รับ Image, Name/Price Text รับ TMP_Text; Lock Icon รับ object รูปแม่กุญแจ เปิดเฉพาะอันที่ล็อก; Selected Indicator และ Equipped Indicator เป็น object แสดงสถานะตามที่คุณออกแบบ
- Equip Button / Unlock Button: ใส่ Button คนละ GameObject เป็นพี่น้องกัน ระบบสลับ SetActive: ชิ้นส่วนล็อกแสดง Unlock อย่างเดียว (เงินไม่พอยังแสดงแต่กดไม่ได้) ชิ้นส่วนปลดแล้วแสดง Equip อย่างเดียว ซื้อสำเร็จจะปิด Unlock และเปิด Equip ทันที ถ้าใส่อยู่แล้ว Equip ยังแสดงแต่กดไม่ได้ ไม่เลือกชิ้นส่วนจะซ่อนทั้งคู่ ใช้ชื่อปุ่ม ENG เช่น Equip, Unlock เก็บ CustomizationUI ไว้บน RepairSystems ที่เปิดอยู่เสมอ ไม่มีช่อง Inspector ใหม่
- Money Text / Selected Name Text / Unlock Price Text: TMP_Text เงิน ชื่อชิ้นส่วนที่เลือก และราคา เฉพาะชิ้นส่วนที่ยังล็อก

รายการในหมวดเรียงชิ้นส่วนที่ปลดแล้วก่อน ตามลำดับใน catalog แล้วต่อท้ายด้วยชิ้นส่วนที่ล็อกใน Container เดียว เมื่อซื้อจะย้ายเข้ากลุ่มปลดแล้วทันที ไม่หักเงินซ้ำและไม่ใส่ให้อัตโนมัติ กด Equip เพื่อใส่ ส่วนของยานที่ตรงหมวดที่เลือกจะสว่างและลอยขึ้นเล็กน้อย ใช้ MaterialPropertyBlock ไม่แก้ Material asset เดิม

RepairUI ผูก ShipCustomization เพื่อปิด Main Menu Panel เมื่อเปิดหน้าปรับแต่ง และกันเริ่มซ่อม/เข้าต่อสู้ซ้อนกัน RepairOrbitCamera เพิ่ม Customization / Customization Camera ผูก CustomizationCamera แยกไว้แล้ว ปรับมุมกล้องนี้เองได้โดยไม่กระทบกล้องเมนู/ซ่อม Canvas ยังใช้ Main Camera จริงตามเดิม

## JSON save

### New Game (2026-10-26)

เพิ่ม `SpaceGarage/Scripts/newGame.cs` (component `SpaceGarage.newGame`) ให้ใส่บน GameObject ของผู้ใช้ แล้วผูก Button > On Click โดยลาก object นั้นและเลือก `newGame.NewGame()` ไม่ได้สร้างปุ่ม/UI หรือผูกปุ่มแทนผู้ใช้

เพิ่มช่อง Inspector `On Start` บน newGame (ค่าเริ่มต้นปิด): ติ๊กเพื่อเรียก NewGame อัตโนมัติใน Start หนึ่งครั้งต่อซีนต่อรอบการเล่น ป้องกันรีเซ็ต/โหลดซีนวน และไม่รีเซ็ตซ้ำเมื่อกลับเข้าซีนเดิมในรอบนั้น ปุ่ม NewGame ยังเรียกซ้ำได้ตามเดิม เริ่ม Play หรือเปิดเกมรอบใหม่จะเริ่มนับใหม่ รวมกรณี Editor ปิด Domain Reload ไม่เปลี่ยนการเปิด/ปิด UI ไม่มี reference ใหม่ให้ลาก

เมื่อเรียกจะใช้ `ResourceManager.ResetForNewGame()` ตั้งเงิน 3000 แทนเงินเดิม ล้างคลังแล้วคืนตาม Starting Materials ของ ResourceManager ที่ใช้งานอยู่ (รวมจำนวนรายการ ID เดียวกัน; ช่องว่าง/จำนวนศูนย์ข้าม) ล้างชิ้นส่วนที่ซื้อทั้งหมด เหลือสิทธิ์ Initially Unlocked ตาม catalog และใส่ชิ้นแรกที่ปลดฟรีของแต่ละหมวดเป็นชุดเริ่มต้น ไม่เปลี่ยน Part ID ของ asset

บันทึก JSON ใหม่ก่อนเปลี่ยนข้อมูลในหน่วยความจำ และอัปเดต .bak ให้เป็น New Game ด้วยเพื่อไม่กู้ความคืบหน้าเก่ากลับมา ถ้าเขียนไฟล์หลักล้มเหลวจะไม่รีเซ็ต/ไม่โหลดซีน ถ้าอัปเดต backup ล้มเหลวจะแจ้ง warning (ไฟล์หลักใหม่ยังใช้ได้) หากไฟล์หลักและ backup อ่านไม่ได้อยู่ก่อนแล้ว ยังคงป้องกันการเขียนทับตามระบบเดิมและแจ้ง warning ไม่ล้างไฟล์จริงในงานพัฒนา

หลังรีเซ็ต โหลดซีนเดิมใหม่แบบ Single และคืน Time.timeScale = 1 เพื่อเคลียร์ลูกค้า/งานซ่อมที่พัก ยานต่อสู้ ศัตรู HP และสถานะชั่วคราว ไม่เปลี่ยนไปซีนอื่น ซีนต้องอยู่ใน Build Profile Scene List (SampleScene และ Repair มีแล้ว) ResourceManager ใช้ Persist Across Scenes/Enable Json Save เดิม: ควรเปิดอย่างน้อยหนึ่งอย่างเพื่อรักษาข้อมูลรีเซ็ตเมื่อโหลดซีน ไม่ได้ปรับค่า prefab หรือ Starting Materials ที่ผู้ใช้ตั้งไว้ กรณี Starting Materials ว่างจะเริ่มด้วยคลังว่าง

ผลตรวจ: คอมไพล์ runtime ทั้งชุดรวม newGame.cs ด้วย Roslyn และ references ของ Unity โปรเจกต์จริงผ่าน exit 0 ตรวจ diff ผ่าน ไม่ได้เรียก NewGame กับเซฟของผู้ใช้ และยังไม่ได้ทดสอบกดปุ่ม/โหลดกลับ/กู้ backup ใน Play Mode ต้องเพิ่ม component และผูก On Click ของปุ่มเองตามด้านบน

ResourceManager เพิ่ม Enable Json Save (เริ่มต้นเปิด) และ Save File Name (space-garage.json) ไฟล์อยู่ `Application.persistentDataPath` มี version, money, materials (Item ID/amount), unlockedParts (Part ID), equippedParts (Gun/ArmorPlate/Wing/Jet) ใช้ ID ไม่เก็บ reference ของ Unity ใน JSON

โหลดอัตโนมัติตอน ResourceManager แรก Awake ข้อมูลเริ่มต้นใช้เมื่อไม่มีเซฟเท่านั้น ไม่แจก Starting Materials ซ้ำหลังโหลด เซฟหลังเปลี่ยนข้อมูลหน่วง 0.5 วินาที และทันทีเมื่อซื้อ/ใส่/ออกหน้าปรับแต่ง/เข้าต่อสู้ รวมถึงตอน pause/ออกเกม เขียนไฟล์ .tmp แล้ว replace ไฟล์จริงพร้อม .bak ถ้าไฟล์หลักเสียจะลอง backup ถ้าทั้งสองอ่านไม่ได้จะเตือนและไม่เขียนทับไฟล์เสีย

เงิน วัสดุ การปลดล็อก และชุดยานใช้ ResourceManager ตัวเดียวข้ามซีน ShipController อ่านชุดที่เซฟก่อนประกอบตอน Awake ถ้าข้อมูลชิ้นส่วนถูกเอาออกจาก catalog จะเลือกชิ้นส่วนที่ปลดแล้วในหมวดนั้นแทน หากไม่มีจะใช้ชิ้นส่วนเดิมของ player prefab

วัสดุที่โหลดแสดงชื่อ/รูปจาก Material Catalog บน ResourceManager ให้ใส่ ShipMaterialData ทุกชนิดที่ใช้ ไม่ต้องใส่ reference วัสดุซ้ำใน JSON ไม่มีการเซฟงานซ่อมลูกค้าที่กำลังค้างหรือการตั้งค่ากล้อง

## ตรวจงาน

คอมไพล์สคริปต์กับ Unity assemblies ผ่าน และตรวจ reference ภายใน Repair.unity ผ่าน ทดสอบโค้ด GarageSaveData/GarageJsonStorage จริงด้วย Mono ผ่าน 21 กรณี: roundtrip เงิน/วัสดุ/ปลดล็อก/ชุดยาน, ตรวจข้อมูลผิด/ID ซ้ำ, ค่าใหญ่, เขียน .tmp/replace, backup และกู้เมื่อไฟล์หลักเสีย ใช้ JSON serializer ของ .NET ในชุดทดสอบนี้ ไม่ใช่ JsonUtility ใน Unity ยังไม่ได้ทดสอบวงจรซื้อ/ใส่/โหลดจริงใน Play Mode หรือกด UI จริง รูป lock และภาพไฮไลต์ต้องตรวจหลังใส่ Canvas/Row Prefab ของผู้ใช้ ลบไฟล์และเครื่องมือตรวจชั่วคราวแล้ว

## Start เริ่มเกมเฉพาะครั้งแรก (2026-10-27)

ResourceManager.BeginGame ใช้ HasStartedGame: โหลดเซฟหลักหรือ .bak สำเร็จถือว่ามีเกมแล้ว รองรับเซฟเดิม ไม่เปลี่ยน schema/Part ID เมื่อมีเกมแล้ว Start ไม่รีเซ็ตเงิน/วัสดุ/ปลดล็อก/ชุดยาน เมื่อไม่มีเซฟจึงเรียก ResetForNewGame เดิมเพื่อเริ่มเงิน 3000, Base และ Starting Materials แล้วเซฟก่อนเปิด Main UI ถ้าเซฟหลักและสำรองอ่านไม่ได้ยังป้องกันเขียนทับและไม่ให้ Start ผ่าน

ก่อนกด Start เมื่อไม่มีเซฟจะไม่ autosave หรือบันทึกตอนปิดแอป เพื่อไม่สร้างไฟล์เกมที่ยังไม่ได้เริ่ม HasStartedGame เป็น runtime ตั้งจากโหลดหรือเริ่มสำเร็จ ไม่เพิ่ม field JSON `newGame.NewGame()` เปลี่ยนเป็น BeginGame ไม่มี auto OnStart/ไม่ reload ซีนแล้ว ลบค่า onStart ใน Repair.unity ส่วน ResetForNewGame ยังคงเป็น API รีเซ็ตโดยตรงที่ต้องเรียกอย่างชัดเจน ปุ่ม Start ไม่เรียกซ้ำ

ผู้ใช้ต้องสร้างและลาก UI เองตาม UI/README.md คอมไพล์ทั้งชุดผ่าน ยังไม่ได้กด UI หรือทดสอบด้วยเซฟจริง
