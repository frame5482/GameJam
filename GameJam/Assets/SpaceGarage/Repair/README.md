# ระบบซ่อมยานลูกค้า

อัปเดต 2026-10-26 — ใช้ Canvas/UGUI ของผู้ใช้ ไม่สร้างหน้าตา UI หรือ UI Toolkit เพิ่ม

## เริ่มตั้งค่า

1. เปิด `Scenes/Repair.unity` เลือก `RepairSystems` มี CustomerRepairManager, RepairInteraction, RepairUI และ RepairOrbitCamera แล้ว จุด IN / Stay / Out เดิมไม่ได้ย้าย และผูกกับ Manager แล้ว
2. เติมวัสดุใน `MaterialPools/GunMaterials.asset`, `ArmorPlateMaterials.asset`, `WingMaterials.asset`, `JetMaterials.asset` แต่ละรายการรับ ShipMaterialData, Units At Full Damage และ Payment Per Unit เลือกจำนวนชนิดที่จะสุ่มด้วย Minimum/Maximum Material Types หากพูลว่างจะไม่เริ่มงานและแจ้งเตือน Console
3. ตั้ง Material Catalog และ Starting Materials บน `ResourceManager.prefab` สำหรับวัสดุเริ่มต้น (จำนวนยังว่างไว้ให้กำหนดเอง) Money คือเงินเริ่มต้น ใช้รหัส Item ID ไม่ซ้ำกันใน ShipMaterialData
4. สร้าง Canvas ของคุณ แล้วลากช่อง UI ลง RepairUI บน RepairSystems ตามรายละเอียดด้านล่าง จากนั้นกด Play และกดปุ่มซ่อมยานลูกค้า

## ช่องบน Inspector

### กด Repair แล้วไม่มียาน

ช่องปุ่มอยู่ที่ `RepairSystems > RepairUI > Repair Customer Button` ผูกปุ่ม Repair แล้ว จึงไม่ต้องเพิ่ม On Click ซ้ำ ตรวจล่าสุดผู้ใช้ใส่ Material ในพูลแล้ว แต่ Units At Full Damage เป็น 0 ทั้งสี่พูล ทำให้ไม่มีสูตรที่ใช้ได้ ต้องตั้งค่านี้มากกว่า 0 เช่น 10 คือใช้ 10 หน่วยที่ความเสียหาย 100% ส่วน Payment Per Unit เป็น 0 ได้หากไม่ต้องการค่าจ้างต่อหน่วย ระบบแสดงคำเตือนแยกชื่อพูล/ช่องที่ไม่ผ่านแล้ว การไม่มีวัสดุในคลังไม่ขวางการรับงาน แต่ต้องมีสูตรในพูลก่อน

Main Menu Panel ใส่ MainUi ได้ ส่วน Repair Panel ยังว่าง ให้ใส่แผงหน้าซ่อมของคุณเมื่อสร้างแล้ว การไม่ใส่ Repair Panel ไม่ได้ขวางการเกิดยาน

- CustomerRepairManager: In Point / Stay Point / Out Point รับ Transform; Customer Prefab รับ CustomerRepairShip; Gun / Armor Plate / Wing / Jet Pool รับ ShipPartStats ของช่องนั้น ส่วน Material Pools รับ RepairMaterialPool ทั้งสี่ช่อง ผูกตัวอย่างให้แล้ว ปรับเปอร์เซ็นต์พัง ความเร็วบิน เวลารอลูกค้าคันถัดไป และค่าจ้างพื้นฐานได้
- CustomerRepairShip บน prefab: จุดติดตั้ง Gun / Armor Plate / Wing / Jet รับ Transform ระบบสร้างชิ้นส่วนทั้งสี่ตอนรับงาน ปิด ShipGun และใช้ไอพ่นปกติระหว่างบิน ไอพ่นดับเมื่อจอด
- RepairInteraction: Workshop และกล้องจริงสำหรับ raycast ผูกแล้ว
- RepairOrbitCamera: Workshop และ Camera Rig เป็น Transform ของ CinemachineCamera เดิม คลิกขวาลากหมุนรอบยาน เลื่อนล้อซูม ทำงานเฉพาะเมื่อเปิดหน้าซ่อม คลิกซ้ายใช้ลากวัสดุ

## ต่อ Canvas ของคุณ

เก็บ RepairUI บน RepairSystems ที่เปิดอยู่เสมอ ไม่ย้ายไปไว้ใต้ Panel ที่จะปิด ระบบเปิด/ปิดแผงด้วย SetActive

- Main Menu Panel / Repair Panel: แผงเมนูและแผงซ่อมของคุณ
- Repair Customer Button / Submit Button: ปุ่มเปิดงานและส่งงาน RepairUI ผูก onClick ให้แล้ว ไม่ต้องผูกซ้ำ หรือจะใช้ CustomerRepairManager.OpenRepair / SubmitRepair ใน UnityEvent เองแทนก็ได้
- Money Text / Payment Text / Job Status Text: TMP_Text เงินที่มี ค่าจ้าง และสถานะงาน
- Inventory Container: RectTransform แถบล่าง จัด Horizontal/Grid Layout เอง; Inventory Item Prefab: prefab UI ที่ติด RepairMaterialDragItem ใส่ Image รูป, TMP ชื่อและจำนวนที่มี ตัว Graphic ต้องรับ raycast เพื่อเริ่มลากได้
- Drag Ghost Prefab บน RepairMaterialDragItem: RectTransform รูปตอนลากที่คุณทำเอง ไม่ใส่ก็ยังส่งวัสดุได้ ระบบไม่สร้างกราฟิกสำรอง
- Tooltip Panel / Part Name Text / Damage Text: แผงเล็กและ TMP ของคุณ แสดงใกล้เมาส์เมื่อชี้ชิ้นส่วน จัดให้เป็นลูก Canvas และตั้งขนาดเอง ระบบใส่ CanvasGroup กัน tooltip บัง raycast
- Requirement Container / Requirement Row Prefab: RectTransform และ prefab ที่ติด RepairRequirementRow ใส่ Image กับ TMP ชื่อ/จำนวน แสดงวัสดุที่ยังขาดและจำนวนที่มี

Canvas ต้องมี GraphicRaycaster ในฉากมี EventSystem พร้อม Input System UI module แล้ว ระบบสร้างแถวจาก prefab ที่คุณให้เท่านั้น ไม่ปรับ layout และไม่ลบลูกอื่นใน Container ช่อง UI ทั้งหมดยังว่างรอของจริง

## กติกาและการเชื่อมระบบ

รับงานแล้วสุ่ม ShipPartStats สี่ช่องและเปอร์เซ็นต์พังของแต่ละชิ้น สุ่มวัสดุไม่ซ้ำรหัสจากพูลของช่องนั้น สูตรซ่อมถูกเก็บตลอดงาน ไม่สุ่มใหม่ตอนชี้เมาส์ จำนวนที่ต้องใช้ = ปัดขึ้น(Units At Full Damage × เปอร์เซ็นต์พัง / 100) เช่น 10 หน่วยที่พัง 35% ใช้ 4 หน่วย

ยานเกิดที่ IN บินไป Stay จึงเริ่มซ่อมได้ ชิ้นส่วนพังใช้สีหม่นและการสั่นแบบ noise ผ่าน MaterialPropertyBlock ไม่แก้ไฟล์ Material เดิม เมื่อชี้จะสว่างและลอยขึ้นเล็กน้อย การเรืองแสงขึ้นกับ shader ที่รองรับ ค่าความเสียหายลดตามสัดส่วนหน่วยวัสดุที่ยังขาด เมื่อครบจะหยุดสั่น

ลากหนึ่งครั้งเติมวัสดุชนิดนั้นเท่าที่ยังขาดและเรามี หักคลังจริง ของผิดชนิด วางพลาด หรือยังจอดไม่เสร็จไม่หักวัสดุ ต้องครบทั้งสี่ชิ้นจึงส่งได้ ค่าจ้าง = Base Payment + ผลรวม(จำนวนวัสดุที่กำหนดตอนรับงาน × Payment Per Unit) จ่ายครั้งเดียวเมื่อกดส่ง ยานบินไป Out และทำลายตัวเอง จากนั้นรับคันถัดไปอัตโนมัติหลัง Next Customer Delay (เริ่มต้น 1 วินาที)

ResourceManager เป็น singleton คงอยู่ข้ามซีน มี ShipInventory เก็บวัสดุและเงิน ใน Repair และ SampleScene วาง prefab ไว้แล้ว ตัวซ้ำจะถูกลบโดยรักษาของเดิม ShipController.Inventory และการเก็บ ShipPickup ใช้คลังกลางนี้ หากระบบอื่นเพิ่มวัสดุ ให้ RegisterMaterial(data) แล้ว Inventory.Add(data.itemId, amount) ใช้ changed สำหรับอัปเดต UI และ TrySpendMoney สำหรับใช้เงิน

ยังไม่มีบันทึกลงดิสก์ ปิดเกมแล้วข้อมูล runtime หาย และยังไม่ทำหน้าปรับแต่งยานหรือเมนูออกสู้ ระบบหยุด/ออกซีนระหว่างซ่อมยังไม่มีบันทึกงานค้างหรือคืนวัสดุ

## การตรวจ

คอมไพล์สคริปต์ SpaceGarage กับ assembly ของ Unity 6000.3.8f1 ผ่าน ตรวจสูตรปัดขึ้น ขอบเขตเปอร์เซ็นต์ จำนวนมาก การเติมบางส่วน และสต็อกไม่พอผ่าน 14 กรณี ทดสอบ API วงจรจริงใน Unity Play Mode ผ่าน 35 จุด: ประกอบสี่ส่วน, ห้ามซ่อมก่อนจอด, ห้ามส่งไม่ครบ, ของผิดชนิด, เติมบางส่วนและหักคลัง, ซ่อมครบ, จ่ายครั้งเดียว, ออกถึง Out และรับคันถัดไปอัตโนมัติ ใช้ข้อมูลชั่วคราวแยกจากคลังจริงและลบเครื่องมือทดสอบแล้ว การทดสอบบังคับตำแหน่งถึงจุดหมาย ไม่ได้ตรวจภาพการเดินทาง ยังต้องทดสอบการลากบน Canvas จริงและภาพการบิน/ไฮไลต์เมื่อใส่ UI และวัสดุของผู้ใช้
