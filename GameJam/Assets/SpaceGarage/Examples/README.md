# Ship part types (2026-10-27)

ชื่อไฟล์ ShipPartStats และ Part prefab เปลี่ยนตามประเภทด้านล่าง โดยย้าย `.meta` คู่กันเพื่อรักษา GUID และ reference เดิม ไม่เปลี่ยน Part ID ของ Starter/Heavy/Barrier ที่อยู่ใน catalog/เซฟแล้ว สำเนา Light/Unity ใหม่ได้รับ ID จาก GUID ของ asset ตัวเองทั้ง 4 ช่องเพื่อไม่ชน Barrier และแก้ Part Prefab ให้ชี้ prefab ในชุดตัวเอง (รวม Light Jet ที่เดิมชี้ Plate)

## ชุดที่เลือกและค่าทดลอง

ตัวคูณใช้กับค่าสเตตัสรวม base + ชิ้นส่วน ค่าทั้งหมดปรับบน **ShipPartStats** ของ asset ได้ ตัวเลขเป็นจุดเริ่มต้นสำหรับปรับสมดุล ยังไม่ได้ทดสอบความรู้สึกการบินจริง

| Folder | Armor Plate | Wing | Jet |
| --- | --- | --- | --- |
| StarterShip | Base Plate: ไม่มี trait | Base Wing: ไม่มี trait | Base Jet: บูสต์ x2 ใช้เกจ 25/s |
| HeavyShip 2 | Heavy Plate: HP x1.8, Speed x0.8, Def +5 | Tank Wing: Speed x0.8, Knockback x0 | Burst Jet: บูสต์ x3.5 ใช้เกจ 40/s |
| Light Ship | Light Plate: HP x0.65, Speed x1.3 | Swept Wing: Speed x1.4, Handling x0.6, Response x1.3 | Cruise Jet: บูสต์ x1.8 ใช้เกจ 10/s |
| BarrierShip | Barrier Plate: โล่ 60 รอ 4s หลังโดน ฟื้น 15/s | Agile Wing: Handling x1.5, Response x0.5 | Twin Jet: หลบ 2 ครั้งต่อชุด |
| Unity Ship | Unity Plate: โหมดเร็ว Speed x1.2; โหมดป้องกัน Speed x0.9, Def +3 | Unity Wing: โหมดเร็ว Speed x1.2; โหมดเลี้ยว Handling x1.25, Response x0.7 | Overdrive: บูสต์ x5 ใช้เกจ 40/s; หลังหยุด Speed x0.6 นาน 2s |

E สลับโหมดของ Unity Plate และ Wing ที่ใส่อยู่พร้อมกัน เริ่มที่โหมดเร็วทุกครั้งที่ประกอบชุด ไม่เซฟโหมดชั่วคราว การสลับไม่เติม HP หรือเกจบูสต์ ไม่มีการสร้าง UI/เปลี่ยนวิธีเปิดปิดแผงหรือแอนิเมชัน

Shift ยังคงเป็นกดค้างใช้เกจเดิม (ฐาน 100) ไม่เปลี่ยนเป็น dash หรือยกเลิกกลางทางตามเวลาตายตัว Burst จึงบูสต์เต็มเกจได้ประมาณ 2.5s, Cruise 10s, Overdrive 2.5s; ระยะจริงขึ้นกับ Speed ของชุดและการหน่วงความเร็วเดิม ตัวคูณ Burst/Overdrive สูงขึ้นเพื่อให้มีระยะมากกว่า Base แม้ใช้เวลาเต็มเกจสั้นกว่า

คูลดาวน์หลบ **1s หลังจบท่า** บน ShipController ทั้งค่าเริ่มต้นและ prefab ที่มีค่าบันทึกเดิม Twin Jet ใช้สองครั้งต่อชุด โดยรอท่าแรก 1.2s จบก่อนครั้งที่สอง แล้วคูลดาวน์ 1s หลังครั้งที่สอง การหลบยัง Space+A/D ระยะ 5 หน่วยใน 0.6s พร้อมหมุน/อมตะ 1.2s เดิม HUD คูลดาวน์เดิมยังทำงาน ไม่มีช่องแสดงจำนวน charge ใหม่

Agile/Unity โหมดเลี้ยวลดเวลารับคำสั่ง/เอียงตัว โดยคงเพดาน pitch 30 และ turn 45 องศา/s ของผู้เล่น Swept ลดเพดานตาม Handling Multiplier ด้วย เพื่อให้เลี้ยวยากจริงแม้ base handling สูง ศัตรูคำนวณอัตราหันจาก Handling รวมตามเดิม

## โล่อัตโนมัติและศัตรู

เพิ่ม `Scripts/ShipPartTraits.cs`: รวมตัวคูณร่วมและ ShipBarrierState ใช้กับ ShipController/Enemy โล่รับดาเมจที่เหลือหลัง Def ก่อน HP ดาเมจล้นโล่ลง HP; ดาเมจที่โดน Def กันหมดไม่ชะลอฟื้นโล่ Dodge ที่อมตะไม่กินโล่ ไม่แสดงเลข/HIT/Hit Reaction ถ้า HP ไม่ลด

Barrier Plate ผูก `VFX/Barrier/Barrier.prefab` แล้ว สร้างเป็นลูก root ยานตอนประกอบ ปิด collider เฉพาะ instance เอฟเฟกต์เพื่อไม่บังกระสุนจริง เปิดด้วย SetActive เมื่อมีโล่ ปิดเมื่อโล่หมด/ยานตาย/controller ปิด และทำลายเมื่อเปลี่ยนชุดหรือยานถูกทำลาย ไม่แก้ shader/กราฟ/ขนาดต้นฉบับ เพิ่ม stripped root reference ใน prefab variant เพื่อให้ ShipPartStats อ้าง GameObject ได้

ศัตรูใช้โล่/ตัวคูณสเตตัส/Tank กันกระเด็นทันที คง Hit Reaction flash และ pause เดิมเมื่อ HP ลด ไม่เพิ่ม AI บูสต์ หลบ หรือสลับ Unity ตามผู้ใช้เลือก Unity ของศัตรูคงโหมดเร็ว เอกสารนี้ไม่กำหนด AI ดังกล่าวแทนผู้ใช้

อัปเดตผู้ใช้ล่าสุด: CustomizationCatalog ฟรีเฉพาะ Base สี่ส่วน ชิ้นอื่น Locked พร้อมราคา รวมรายการครบ 20 ชิ้นทั้งปืนและ Plate/Wing/Jet ดูตารางราคาใน [Customization/README.md](../Customization/README.md) ปืน Light/Barrier/Unity เปลี่ยน Display Name ให้แยกกัน ไม่เพิ่มความสามารถปืน พูล Gun/Armor Plate/Wing/Jet บน EnemySpawnManager prefab และ CustomerRepairManager ใน Scenes/Repair.unity มีครบหมวดละ 5 ชิ้นแล้ว SampleScene สืบทอด EnemySpawnManager prefab โดยไม่มี override พูล ไม่ต้องใส่รายการเอง

## ช่อง Inspector ใหม่

- ShipPartStats > Part Traits: Speed / Health / Handling Multiplier รับตัวคูณรวม; Steering Response Multiplier รับตัวคูณเวลาตอบสนองและความเร็วเอียง; Knockback Multiplier 0–1 (0 กันกระเด็น, 1 ปกติ)
- Unity Mode: Switchable Mode เปิดสลับด้วย E; Alternate Speed / Handling / Steering Response Multiplier และ Alternate Defense ใช้ในโหมดที่สอง
- Automatic Barrier: Barrier Capacity, Recovery Delay (วินาที), Recovery Per Second, Barrier Prefab (GameObject VFX), Barrier Visual Scale (ตัวคูณขนาด instance)
- Jet Special Settings: Dodge Charges, Post Boost Speed Multiplier, Post Boost Slow Duration (วินาที); Boost Speed/Drain/Recovery/Delay เป็นช่องเดิม
- ShipPickup > Attraction Speed ค่าเริ่มต้น 25 หน่วย/s บวกความเร็วยาน ให้ดูดตามยานทัน เริ่มดูดอัตโนมัติเมื่อเข้า Pickup Radius/Layers เดิมบน ShipController และเพิ่มคลังเมื่อถึงระยะ 0.5 หน่วย ไม่ใช้ E แล้ว เก็บครั้งเดียวแม้หลาย collider

ใส่ค่าและ reference ของทั้ง 15 ชิ้นให้แล้ว ไม่ต้องลากค่าเพิ่มเพื่อใช้กับผู้เล่น ไม่เพิ่มช่องบน ShipHUD ไม่สร้าง UI แสดงโล่/โหมดใหม่

## ผลตรวจ

คอมไพล์ runtime ทั้งชุดด้วย Roslyn กับ references จริงของ Unity ผ่าน exit 0 (รอบสุดท้ายไม่มี warning) ทดสอบ source จริงของ ShipDodgeState/ShipPartTraits/ShipBarrierState ด้วย Unity shim แบบกำหนดเวลาเองผ่าน 23 กรณี รวมช่วงอมตะ คูลดาวน์ 1s/Twin โล่ดูดดาเมจ/ล้น/รอฟื้น/ไม่ฟื้นตอนตาย/เพดาน และ Unity สองโหมด ตรวจ 20 Part ID ไม่ซ้ำ, prefab reference มี root fileID 100 ครบ, catalog Plate/Wing/Jet อย่างละ 5 และ serialized cooldown 1s ผ่าน ตรวจ diff เฉพาะไฟล์งานนี้ผ่าน ลบเครื่องมือ/ผลคอมไพล์ชั่วคราวแล้ว

ยังไม่ได้ทดสอบ Play Mode/เรนเดอร์ภาพโล่บนยานจริง โดยเฉพาะขนาด VFX การดูดของ และความรู้สึกการบินต้องประเมินใน Unity หากต้องการปรับใช้ Barrier Visual Scale ที่ BarrierPlate.asset ไม่ต้องตั้งค่าเพิ่มเพื่อเริ่มใช้งาน หากใช้ EnemySpawnManager ที่สืบทอด prefab ตัวอย่างโดยไม่ override พูล จะได้รับรายการใหม่อัตโนมัติ
