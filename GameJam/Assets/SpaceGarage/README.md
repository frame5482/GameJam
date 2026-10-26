# ระบบควบคุมยานต้นแบบ

อัปเดต 2026-10-26 — Unity 6000.3.8f1, Input System 1.18.0, บินแบบเครื่องบิน 3D เปลี่ยนระดับความสูงแกน Y ได้จริง ไม่ล็อกระนาบ

## พฤติกรรมที่ยืนยัน

- ยานบินไปข้างหน้าตลอดเวลาตามหัว (local +Z) ด้วย Speed แม้ไม่ได้กดปุ่ม ปล่อยปุ่มแล้วบินต่อในทิศปัจจุบัน
- S เชิดหัวไต่ระดับ (+Y), W กดหัวลดระดับ (-Y) เทียบกับท่าบินปกติ เป็นการหมุนก้ม/เงยต่อเนื่อง ไม่ใช่สั่งหันไป +Z/-Z กดค้างสามารถตีลังกาได้
- A/D เอียงลำซ้าย/ขวาแล้วเปลี่ยนเส้นทางบินตามมุมเอียงจริง; ปล่อยค่อยคืนปีกตรง และยังบินต่อในทิศใหม่ ไม่หันไปแกนโลก -X/+X โดยตรง
- Handling กำหนดอัตราก้ม/เงยและอัตราเลี้ยวขณะเอียงเต็ม หน่วยองศาต่อวินาที ยิงตามหัว ไม่เล็งเมาส์
- มุมเอียงเริ่มต้น 25 องศา หมุนลำยานทั้งชุดรวม collider ด้วย Rigidbody ไม่ใช่แค่เอียงกราฟิก
- กด Space ค้างเพื่อบูสต์ขณะเคลื่อนที่ เกจเริ่มต้น 100 ใช้ต่อวินาทีตาม Jet ปล่อยปุ่มแล้วฟื้นหลังเวลาหน่วง ถ้าเกจหมดต้องปล่อยก่อนใช้ครั้งใหม่
- คลิกซ้ายค้างยิงตาม local +Z ของ Muzzle
- E เก็บของในรัศมีที่กำหนดลง ShipInventory ของที่เก็บสำเร็จหายไป ไม่เพิ่มซ้ำเมื่อมีหลาย collider
- ไม่มี UI ชุดทดลองเดิมไม่มีโมเดลยาน ส่วน StarterShip เพิ่มรูปทรงพื้นฐานตามคำขอภายหลัง ใช้ทรงกลมเป็นกระสุน

## ไฟล์และหน้าที่

| ไฟล์ | หน้าที่ |
| --- | --- |
| Scripts/ShipController.cs | คุมอินพุต เคลื่อนที่ หันหัว เลือด Def เกจบูสต์ เก็บของ รวมสเตตัสและประกอบชิ้นส่วน |
| Scripts/ShipPartStats.cs | ScriptableObject ประเภทชิ้นส่วน Prefab สเตตัส และค่าบูสต์ของ Jet |
| Scripts/ShipGun.cs | ใส่ใน Prefab ปืน กำหนดกระสุน จุดยิง ความถี่และตัวคูณดาเมจ |
| Scripts/ShipProjectile.cs | เคลื่อนกระสุน ตรวจชนแบบ sphere sweep รับดาเมจจากปืน ไม่โดนเจ้าของ ไม่ใช้ Rigidbody |
| Scripts/IShipDamageable.cs | สัญญาสำหรับสิ่งที่รับดาเมจผ่าน TakeDamage(float) |
| Scripts/ShipInventory.cs | เก็บจำนวนวัสดุตาม Item ID ในรอบเล่น |
| Scripts/ShipPickup.cs | ใส่ของดรอป กำหนด Item ID และจำนวน ให้ E เก็บได้ |
| Editor/ShipPrototypeBuilder.cs | เมนูสร้างซีนและ Prefab ทดลองในโฟลเดอร์ใหม่ ไม่เขียนทับซีนเดิม |

## ทดลองแบบเตรียมให้

### ตัวอย่างที่วางในซีนแล้ว (2026-10-26)

วาง StarterShip แบบ Prefab Instance ที่ `(0,0,0)` ใน `Assets/Scenes/SampleScene.unity` ชื่อ **PlayerShip (ShipController)** เลือก root นี้เพื่อดู ShipController, Rigidbody, BoxCollider และ ShipInventory ได้เลย ส่วนลูกมี GunSpawnPoint / ArmorPlateSpawnPoint / WingSpawnPoint / JetSpawnPoint ที่ผูกตำแหน่งเสกครบ 4 ช่องแล้ว

| ช่องบน ShipController | สิ่งที่ผูกให้ |
| --- | --- |
| Gun Part / Gun Mount | Gun.asset / ลูก GunSpawnPoint |
| Armor Plate Part / Armor Plate Mount | ArmorPlate.asset / ลูก ArmorPlateSpawnPoint |
| Wing Part / Wing Mount | Wing.asset / ลูก WingSpawnPoint |
| Jet Part / Jet Mount | Jet.asset / ลูก JetSpawnPoint |
| Inventory | ShipInventory บน root ยาน |

ข้อมูลแต่ละ asset มี Part Prefab ของตัวเอง; ใน GunPart มี ShipGun ผูก Muzzle และ StarterBullet.prefab ไว้แล้ว ไม่ต้องลากช่องเพิ่มเพื่อประกอบตัวอย่าง ตรวจโครงสร้างไฟล์และ reference ของ instance แล้ว แต่ยังไม่ได้เปิดซีนตรวจภาพ/ทดสอบ Play Mode จริง กล้องเดิมของ SampleScene เป็นกล้องนิ่ง ไม่ติดตามยาน เมื่อบินออกนอกกรอบจะมองไม่เห็น ต้องจัดกล้องให้เหมาะกับการบิน 3D ก่อนทดลองใน Game View

### Prefab ประกอบสำเร็จ StarterShip (เพิ่มตามคำขอ 2026-10-26)

แก้การอ้างอิง Material ตามรายงานยานสีชมพู: Prefab เดิมเขียน external reference ของ `.mat` และ `.asset` เป็น `type: 3` ซึ่งเป็นชนิดอ้างอิงไฟล์นำเข้า แก้เป็น `type: 2` สำหรับ NativeFormat asset ครบ StarterShip, ชิ้นส่วนทั้ง 4 และ StarterBullet รวมถึงข้อมูลสเตตัส 4 ช่อง โดยคง GUID เดิมทั้งหมด ตรวจว่า Material มีจริง Shader GUID ตรงกับ URP Unlit ใน package และตรวจไม่เหลือ native reference ชนิดผิดแล้ว ยังไม่ได้ตรวจภาพใน Unity จริง ไม่เพิ่มช่อง Inspector และไม่ต้องลาก Material ใหม่ถ้าไม่ได้ override Renderer ใน instance เอง

- ใช้ `Assets/SpaceGarage/Examples/StarterShip/StarterShip.prefab` ลากลงซีนได้เลย ไม่ต้องกดเมนูสร้างชุดทดลองก่อน
- มีรูปทรงพื้นฐานสีฟ้า ปืนด้านหน้า เกราะกลาง ปีกซ้ายขวารวมเป็น Wing ส่วนเดียว และเครื่องยนต์คู่ด้านหลัง พร้อมวัสดุ URP Unlit 4 สี
- ผูกข้อมูล Gun.asset / ArmorPlate.asset / Wing.asset / Jet.asset และ Prefab ของแต่ละส่วนไว้ครบ พร้อม StarterBullet.prefab ทรงกลม ปืนยิงไป local +Z
- จุดเสกอยู่บนลูกยานชื่อ GunSpawnPoint `(0, 0.14, 0.85)`, ArmorPlateSpawnPoint `(0, 0, 0)`, WingSpawnPoint `(0, 0, -0.1)`, JetSpawnPoint `(0, 0, -1.05)` ทั้ง 4 จุดผูกกับช่อง Mount บน ShipController แล้ว ขยับได้ตามต้องการ
- เห็นชิ้นส่วนประกอบใน Edit Mode ได้เลย โดย ShipController เก็บ reference ชิ้นตัวอย่างในฟิลด์ซ่อน assembledParts เมื่อ Awake จะปิดและถอดชุดตัวอย่างก่อนประกอบจริงจากข้อมูล เพื่อไม่ให้เกิดส่วน/ปืนซ้ำ ไม่มีช่อง Inspector ใหม่ที่ผู้ใช้ต้องกรอก
- ลาก Prefab ลงซีนที่มีกล้องมองเห็นยาน แล้วกด Play เพื่อทดลองบิน 3D กล้องยังไม่ติดตามยาน ไม่ได้ปรับกล้องในงานแก้การควบคุม
- ค่าเพิ่มสเตตัสแต่ละส่วนเริ่มที่ 0 ใช้ค่าพื้นฐานยาน Attack 10 / Def 0 / Speed 8 / Handling 360 / HP 100 / Boost 100 ก่อน ผู้ใช้เปลี่ยน Prefab/Stats ใน asset ได้เอง
- ตรวจ reference ภายใน/ข้าม asset ของไฟล์ตัวอย่าง 14 ไฟล์และตรวจคอมไพล์ผ่าน ไม่มี error/warning ยังไม่ได้ดูภาพหรือทดสอบ Play Mode ใน Unity จริง

### เมนูสร้างชุดทดลองเปล่าเดิม

1. รอ Unity คอมไพล์ เลือกเมนู **Space Garage > Create Control Prototype**
2. เปิดซีน `Assets/SpaceGarage/Prototype/ControlDemo/ControlDemo.unity` ที่ถูกสร้าง (ถ้าสร้างซ้ำชื่อโฟลเดอร์จะมีเลขต่อท้าย)
3. PlayerShip ไม่มี Renderer ตามขอบเขตงาน ให้ใส่โมเดลของคุณเป็นลูกของ Prefab ชิ้นส่วน โดยหัวหัน local +Z หรือเปิด Gizmos และเลือกยานใน Scene View ขณะทดสอบเพื่อเห็น collider และรัศมีเก็บของ
4. กด Play และคลิก Game View ให้มีโฟกัส ใช้ปุ่มด้านบน การกด E ใกล้ลูกกลมที่ (2,0,2) เพิ่ม Scrap 1 ใน Items บน ShipInventory
5. แก้ Jet.asset เพื่อเปรียบเทียบอัตราสิ้นเปลือง/ความเร็วบูสต์ ปรับสเตตัสแล้วเริ่ม Play ใหม่ หรือเรียก RebuildStats() ระหว่างเล่น

ชุดทดลองประกอบด้วย PlayerShip.prefab, Bullet.prefab, ScrapPickup.prefab, Prefab ชิ้นส่วน 4 อัน และข้อมูลชิ้นส่วน 4 asset ที่ผูกไว้แล้ว ส่วน Armor/Wing/Jet เป็น GameObject เปล่าให้ใส่กราฟิกเอง ค่าเพิ่มสเตตัสเริ่มต้นของทุกส่วนเป็น 0 เพื่อคงค่าพื้นฐานยานไว้ก่อน

## ตั้งค่าเองและช่อง Inspector

### ShipPartStats: ข้อมูลชิ้นส่วนแต่ละแบบ

สร้างด้วย **Create > Space Garage > Ship Part Stats** สร้างกี่แบบก็ได้ ไม่แก้ asset สเตตัสร่วมเพื่อเก็บสถานะเฉพาะยาน

- **Slot**: เลือก Gun / ArmorPlate / Wing / Jet ไม่มี Nose Cone
- **Part Prefab**: ลาก Prefab ชิ้นส่วนนั้น ปืนต้องมี ShipGun และจุด Muzzle อยู่ภายใน Prefab
- **Stats**: Attack, Defense, Speed, Handling (องศา/วินาที), Health, Booster Capacity (ขนาดเกจ ไม่ใช่จำนวนชาร์จ)
- ทุกส่วนเพิ่มสเตตัสได้ทุกช่อง ค่ารวม = Base Stats + Stats ของทั้ง 4 ส่วน
- **Jet settings**: Boost Speed Multiplier, Boost Drain Per Second, Boost Recovery Per Second, Boost Recovery Delay ใช้เฉพาะ asset ที่อยู่ในช่อง Jet
- เช่น ยานพื้นฐานมีเกจ 100 และ Jet เพิ่ม Booster Capacity 20 จะได้เกจเต็ม 120 ถ้าต้องการให้เกจยังเป็น 100 ให้ค่าที่เพิ่มเป็น 0

### ShipController: ติดที่ root ยาน

- ต้องมี Rigidbody และ collider แบบ 3D บนยาน สคริปต์ปิด Gravity และปลด Constraints ทั้งหมดใน Awake เพื่อบินขึ้นลง ก้มเงย และเอียงได้จริง ไม่ใช้ Rigidbody2D
- **Base Stats**: Attack 10, Defense 0, Speed 8, Handling 360, Health 100, Booster Capacity 100
- **Gun Part / Armor Plate Part / Wing Part / Jet Part**: ลาก ShipPartStats ให้ตรง Slot ช่องละ 1 asset
- **Gun Mount / Armor Plate Mount / Wing Mount / Jet Mount**: ลาก Transform ลูกของยานที่เป็นจุดติดตั้ง จะวางชิ้นส่วนที่ local position/rotation เป็นศูนย์บน mount และคงสเกลของ Prefab ถ้าเว้นว่างใช้ root ยาน
- จัดตำแหน่ง mount เอง ปีกคู่ให้รวมซ้าย/ขวาใน Wing Prefab เดียว ชิ้นส่วนไม่ควรมี Rigidbody หรือ ShipController ซ้อน ให้ฟิสิกส์อยู่ที่ root ยาน
- **Guns**: ระบบค้นปืนในลูกยานใหม่หลังประกอบ ไม่ต้องลากเอง
- **Inventory**: ลาก ShipInventory หรือปล่อยว่างให้ค้นบน GameObject เดียวกัน
- ลบช่อง **Bank Visual Root** และวิธีสร้าง ShipVisualBank ออกแล้ว การเอียงทำที่ Rigidbody root จริง ส่วน mount ทั้ง 4 อยู่ตาม hierarchy ที่จัดไว้ ไม่ต้องลากช่องทดแทน
- **Bank Angle / Bank Degrees Per Second**: มุมเอียงสูงสุดและความเร็วเอียง/คืนตรง ค่าเริ่มต้น 25 องศาและ 120 องศาต่อวินาที ปรับได้บน ShipController
- **Pickup Radius / Pickup Layers**: ระยะเก็บและเลเยอร์ของ collider ของดรอป
- **Health Changed / Boost Changed**: อีเวนต์ส่งค่าปัจจุบันกับค่าสูงสุดให้ UI ที่ผู้ใช้ทำในอนาคต; **Died**: อีเวนต์ตอนเลือดหมด

เมื่อ Awake ระบบประกอบ Prefab 4 ส่วนตามข้อมูลที่ใส่ไว้โดยอัตโนมัติ ส่วนที่ไม่ใส่ asset/Prefab จะไม่ถูกสร้าง และ asset ผิด Slot จะถูกข้ามพร้อมคำเตือน ไม่สร้างกราฟิกสำรอง

### ShipGun: ติดใน Prefab ส่วน Gun

- **Projectile Prefab**: ลาก Prefab ที่มี ShipProjectile (ชุดทดลองเป็นลูกกลม)
- **Muzzle**: Transform จุดยิง local +Z คือทิศกระสุน ถ้าไม่ใส่ใช้ Transform ของ ShipGun
- **Shots Per Second / Damage Multiplier / Projectile Speed / Projectile Lifetime**: ความถี่ยิง ตัวคูณ Attack ความเร็ว และอายุกระสุน
- ดาเมจกระสุน = Attack รวมของยาน × Damage Multiplier ไม่มีการใช้ Attack จากชิ้นปืนซ้ำสองรอบ
- Projectile มี Hit Radius และ Hit Layers เป้าหมายใช้ collider ปกติ ไม่ใช่ trigger และ implement IShipDamageable ถ้าต้องการรับดาเมจ กำแพงที่ไม่รับดาเมจก็หยุดกระสุนได้

### ShipPickup / ShipInventory

- ของดรอปใช้ ShipPickup กับ SphereCollider ตั้ง Is Trigger ไว้ กำหนด Item ID เช่น Scrap และ Amount
- วาง collider ให้ศูนย์กลางตรงวัตถุ ของเก็บต้องอยู่ใน Pickup Layers ของยาน
- ShipInventory.Items เก็บรายการในหน่วยความจำ; Count(itemId) อ่านจำนวน และ Add(itemId, amount) เพิ่มจำนวน; Changed แจ้งให้ UI อัปเดต

## เชื่อมหน้าปรับแต่งภายหลัง

เรียก `controller.SetLoadout(gun, armorPlate, wing, jet)` ด้วย ScriptableObject ทั้ง 4 ตัว รับ null ได้ ถ้าประเภทไม่ตรงช่องจะคืน false และคงชุดเดิม

ระหว่างเล่นจะปิด/ถอดชิ้นเก่า ประกอบ Prefab ใหม่ ค้น ShipGun ใหม่ และรวมสเตตัสใหม่ โดยไม่เติมเลือด/เกจฟรี (คงค่าปัจจุบันและจำกัดไม่ให้เกินค่าสูงสุดใหม่) หากตั้งก่อน Awake ระบบจะประกอบตอนเริ่มเล่น

`RebuildStats()` รวมค่าสเตตัสใหม่อย่างเดียว ไม่เปลี่ยน Prefab; `RebuildStats(true)` เติมเลือด/เกจเต็มสำหรับการเริ่มรอบใหม่ที่ระบบเกมอนุญาต

## ข้อจำกัดและผลตรวจ

- แก้การควบคุมตามผู้ใช้ยืนยันล่าสุดเป็นเครื่องบิน 3D: S เชิด / W กดหัว / A,D เอียงแล้วเลี้ยว บินต่อเนื่อง ปลด Constraints ของ Rigidbody และลบ Bank Visual Root ตัวอย่างเดิมไม่ต้องตั้งค่าเพิ่ม ค่ามุมเอียง 0 ทำให้ไม่เลี้ยวด้วย A/D เพราะไม่มีการเอียง

- ตรวจคอมไพล์สคริปต์ Runtime และ Editor กับ DLL จริงของ Unity 6000.3.8f1 และ Input System ด้วย Roslyn ผ่าน ไม่มี error/warning
- ยังไม่ได้ทดสอบ Play Mode หรือดูภาพจริง จึงยังไม่ยืนยันความรู้สึกควบคุม/การชน/ภาพจากกล้อง
- สูตร Def ต้นแบบ: ความเสียหายที่ได้รับ = max(0, ดาเมจเข้า - Defense); เลือดหมดหยุดคุมและยิง เรียก Died ไม่ลบยานหรือสร้างหน้าแพ้เอง
- ยังไม่มีระบบมอนสเตอร์สุ่มดรอป เซฟคลัง/โหลดชุดแต่ง หน้าแต่งยาน Barrier และพฤติกรรมเฉพาะปืน 5 แบบ ใช้ปืนยิงกระสุนตรงเป็นต้นแบบก่อน
- ค่าฟื้นเกจ ตัวคูณบูสต์ สูตร Def และระยะเก็บเป็นค่าต้นแบบที่ปรับต่อได้ ไม่ใช่ข้อสรุปบาลานซ์เกม
- ไม่มีการแก้ SampleScene, UI, Input Actions เดิม หรือ Project Settings เมนูสร้างชุดทดลองต้องกดใน Unity เอง
