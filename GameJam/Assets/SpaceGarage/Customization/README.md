# Ship Customization and JSON Save

อัปเดต 2026-10-26 ใช้ Canvas ของผู้ใช้ ข้อความบน UI เป็น ENG ไม่สร้างหน้าตา Panel หรือจัด layout แทน

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

ResourceManager เพิ่ม Enable Json Save (เริ่มต้นเปิด) และ Save File Name (space-garage.json) ไฟล์อยู่ `Application.persistentDataPath` มี version, money, materials (Item ID/amount), unlockedParts (Part ID), equippedParts (Gun/ArmorPlate/Wing/Jet) ใช้ ID ไม่เก็บ reference ของ Unity ใน JSON

โหลดอัตโนมัติตอน ResourceManager แรก Awake ข้อมูลเริ่มต้นใช้เมื่อไม่มีเซฟเท่านั้น ไม่แจก Starting Materials ซ้ำหลังโหลด เซฟหลังเปลี่ยนข้อมูลหน่วง 0.5 วินาที และทันทีเมื่อซื้อ/ใส่/ออกหน้าปรับแต่ง/เข้าต่อสู้ รวมถึงตอน pause/ออกเกม เขียนไฟล์ .tmp แล้ว replace ไฟล์จริงพร้อม .bak ถ้าไฟล์หลักเสียจะลอง backup ถ้าทั้งสองอ่านไม่ได้จะเตือนและไม่เขียนทับไฟล์เสีย

เงิน วัสดุ การปลดล็อก และชุดยานใช้ ResourceManager ตัวเดียวข้ามซีน ShipController อ่านชุดที่เซฟก่อนประกอบตอน Awake ถ้าข้อมูลชิ้นส่วนถูกเอาออกจาก catalog จะเลือกชิ้นส่วนที่ปลดแล้วในหมวดนั้นแทน หากไม่มีจะใช้ชิ้นส่วนเดิมของ player prefab

วัสดุที่โหลดแสดงชื่อ/รูปจาก Material Catalog บน ResourceManager ให้ใส่ ShipMaterialData ทุกชนิดที่ใช้ ไม่ต้องใส่ reference วัสดุซ้ำใน JSON ไม่มีการเซฟงานซ่อมลูกค้าที่กำลังค้างหรือการตั้งค่ากล้อง

## ตรวจงาน

คอมไพล์สคริปต์กับ Unity assemblies ผ่าน และตรวจ reference ภายใน Repair.unity ผ่าน ทดสอบโค้ด GarageSaveData/GarageJsonStorage จริงด้วย Mono ผ่าน 21 กรณี: roundtrip เงิน/วัสดุ/ปลดล็อก/ชุดยาน, ตรวจข้อมูลผิด/ID ซ้ำ, ค่าใหญ่, เขียน .tmp/replace, backup และกู้เมื่อไฟล์หลักเสีย ใช้ JSON serializer ของ .NET ในชุดทดสอบนี้ ไม่ใช่ JsonUtility ใน Unity ยังไม่ได้ทดสอบวงจรซื้อ/ใส่/โหลดจริงใน Play Mode หรือกด UI จริง รูป lock และภาพไฮไลต์ต้องตรวจหลังใส่ Canvas/Row Prefab ของผู้ใช้ ลบไฟล์และเครื่องมือตรวจชั่วคราวแล้ว
