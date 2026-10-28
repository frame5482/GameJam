# Combat HUD ตัวอย่าง — UI Toolkit

## ข้อความอธิบายการควบคุม (2026-10-27)

เพิ่มช่อง **Controls Text** บน MainUI > ShipHUD รับ TMP_Text ที่ผู้ใช้สร้าง และ **Controls Description** เป็นข้อความหลายบรรทัดแก้ได้ใน Inspector ระบบใส่ข้อความลง Text เมื่อเปิด ShipHUD ไม่เปิด/ปิด GameObject ไม่สร้าง Text/Panel/Toolkit ไม่ย้าย layout และไม่เพิ่มแอนิเมชัน

ข้อความเริ่มต้นเป็นอังกฤษตามปุ่มจริง: บินหน้าอัตโนมัติ, W/S กดหัว/เชิดหัว, A/D เอียงเลี้ยว, Shift ค้างบูสต์, Space+A/D หลบ, คลิกซ้ายค้างยิงตามเมาส์, คลิกขวาค้างลากหมุนกล้อง, E สลับ Unity mode และเก็บวัสดุอัตโนมัติ หากต้องการแก้คำอธิบายให้แก้ Controls Description; ข้อความที่พิมพ์บน TMP_Text เองจะถูกแทนด้วยค่านี้ตอนเปิด HUD

ผู้ใช้ต้องลาก TMP_Text ลง **Controls Text** เพียงช่องเดียว และจัดขนาด/ตำแหน่ง/ฟอนต์เอง หากไม่ผูกจะไม่แสดงและไม่สร้างของสำรอง ไม่มี reference ใหม่ที่ผูกให้ในซีนเพราะยังไม่ระบุ Text ตัวจริง คอมไพล์ runtime ด้วย Roslyn/references Unity ผ่าน ยังไม่ได้ตรวจภาพใน Play Mode

## แก้ Game Over ไม่เปิดเมื่อใช้แผงร่วม (2026-10-27)

พบ SampleScene ผูก Game Over Panel และ Success Panel เป็น GameObject เดียวกัน (รวม Title/Back Button/สถิติ/Container ก็ใช้ร่วมกัน) โค้ดเก่าตอนแพ้เปิด Game Over แล้วปิด Success ซึ่งเป็น object เดียวกัน จึงปิดผลลัพธ์ทันที แก้ `ExplorationRun.Finish` ให้ปิดอีกแผงเฉพาะเมื่อเป็นคนละ object และเปิดแผงผลลัพธ์ที่ตรงเพียงครั้งเดียว พร้อมตั้ง Title ตอนจบตามผลจริงเพื่อไม่ให้ข้อความร่วมค้างเป็น Exploration Complete ตอนแพ้

ใช้ SetActive เดิม ไม่มีเฟด ไม่แก้ scene/reference/layout หรือเพิ่มช่อง ผู้ใช้ไม่ต้องแยกแผงหรือผูกใหม่ รองรับทั้งแผงร่วมและแผงแยก คอมไพล์ runtime กับ Roslyn/references Unity จริงผ่าน ตรวจ reference แผงร่วมและลำดับเปิด/ปิดทั้งแพ้/สำเร็จแล้ว ยังไม่ได้ทดสอบตาย/เรนเดอร์ UI จริงใน Play Mode ให้เริ่ม Play ใหม่เพื่อโหลดโค้ดที่แก้

อัปเดต 2026-10-27 วัสดุจากศัตรู: ฆ่าแล้วรับวัสดุจากพูลหน้าซ่อม 1 ชิ้นเข้าคลังทันที ส่ง pickupCollected ชุดเดียวกับการเก็บ ShipPickup จึงขึ้น RepiarMat ใน Pickup Container และถูกรวมในรายการ/ยอด Collected ของผลลัพธ์เดิมทั้งสองจุด ไม่สร้าง UI หรือ Container ใหม่ ไม่ต้องลากช่อง UI เพิ่มหากผูกไว้แล้ว ไม่ส่ง event ถ้าเพิ่มคลังล้มเหลว รายละเอียด Enemy > Drop Material Pools และผลตรวจใน README หลัก

## ยอดฆ่าและยอดเก็บในผลลัพธ์ (2026-10-27)

เพิ่มช่อง TMP_Text บน **MainUI > ExplorationRun** ให้ผู้ใช้ลากข้อความของทั้งสองแผงเอง:

- **Game Over Kills Text / Success Kills Text**: แสดง `Kills: 12` นับศัตรูที่ผู้เล่นยิงจน HP เหลือ 0 ในรอบนี้
- **Game Over Collected Text / Success Collected Text**: แสดง `Collected: 120` รวมจำนวนวัสดุที่ ShipPickup เก็บสำเร็จรอบนี้ (นับจำนวนชิ้น ไม่ใช่จำนวนชนิดหรือจำนวนครั้งที่เก็บ) ไม่รวมคลังเดิมและโบนัสซึ่งแสดงท้ายแถววัสดุแยกอยู่แล้ว

ตัวเลขแสดงเมื่อจบรอบทั้งแพ้และสำเร็จ ก่อนเปิด Panel เดิม ไม่สร้าง Text/Panel/กราฟิกแทน ไม่มีการย้าย layout หรือเปลี่ยนวิธีเปิด/ปิด UI ผู้ใช้ต้องทำและลาก TMP_Text ลง 4 ช่องใหม่; ถ้าใช้ Text ร่วมระหว่างสองผลลัพธ์สามารถลากตัวเดียวกันลงคู่ช่องได้

แก้ `Enemy.cs` ส่ง event Defeated พร้อมฝ่ายผู้โจมตีเมื่อ HP ลดจนตายจริง (ก่อน died เดิม) `ExplorationRun` รับเฉพาะฝ่าย Player และ Enemy ในซีนเดียวกันขณะรอบยังไม่จบ ไม่เพิ่มยอดตอน Enemy แค่ถูกปิด/ทำลาย/ออกซีนหรือโดนฝ่ายอื่นฆ่า ถอด listener เมื่อจบ/ปิด/ทำลาย controller และ reset static listener ตอนเริ่ม Play รองรับปิด Domain Reload ยอดไม่ถูกเซฟหรือยกไปรอบถัดไป

ผลตรวจ: คอมไพล์ runtime ทั้งชุดด้วย Roslyn/references Unity จริงผ่าน ตรวจเส้นทาง HP=0/ShipHitSource.Player/ขอบเขตซีน/HasEnded และการใช้ยอด Collected เดิมที่แยกโบนัสแล้ว ตรวจช่องใหม่ใน SampleScene ยังไม่ได้ทดสอบฆ่า/จบรอบหรือดูข้อความจริงใน Play Mode ต้องลาก Text ของผู้ใช้ก่อนเห็นบนจอ

## รายการของที่ได้รับตอนจบ และรูปโบนัส (2026-10-27)

หน้าผลลัพธ์แพ้/สำเร็จแสดงวัสดุที่เก็บสำเร็จรอบนี้รวมชนิดเดียวกันเป็นหนึ่งแถว ใช้ `UI/pre/RepiarMat.prefab` เดิม เมื่อมีโบนัสชนิดนั้น จำนวนโบนัสต่อท้ายในแถวเดียว เช่น `+20 +2` และเปิดรูปโบนัสเฉพาะแถวนั้นด้วย SetActive ถ้าไม่มีโบนัสแสดงเพียง `+20` และปิดรูป แพ้แสดงยอดที่เก็บได้และไม่มีโบนัส ไม่แสดงคลังเดิม ไม่รวมโบนัสทบเป็นยอดเก็บปกติ

ช่องใหม่บน **MainUI > ExplorationRun**:

- **Game Over Materials Container**: RectTransform พื้นที่รายการบนแผงแพ้ของคุณ
- **Success Materials Container**: RectTransform พื้นที่รายการบนแผงสำเร็จของคุณ ถ้าใช้พื้นที่ร่วมกันสามารถลาก Container เดียวกันได้
- **Result Row Prefab**: RepairMaterialDragItem บน prefab รายการวัสดุ ผูก RepiarMat ให้แล้วใน SampleScene

ช่องใหม่บน **RepairMaterialDragItem ของ prefab RepiarMat**: **Bonus Icon** รับ GameObject ลูกที่มีรูปโบนัสของคุณ สร้าง/วางรูปในแถวตามตำแหน่งที่ต้องการแล้วลากลงช่องนี้ ห้ามใส่ root แถวหรือ Container เอง ระบบเปิดเฉพาะเมื่อโบนัสที่เพิ่มเข้าคลังจริงมากกว่า 0 และปิดในแถวหน้าซ่อม/แจ้งเตือนเก็บของปกติผ่าน Bind เดิม ไม่มีการสร้างรูปแทนหรือแก้ layout ให้

เพิ่ม `ExplorationRewards.CollectedItems` เก็บยอดแยกชนิดแบบ long และ cache ShipMaterialData ตอน pickup ใน ExplorationRun เพื่อแสดงรูป/ชื่อได้แม้ไม่มีใน catalog เก็บยอดโบนัสเฉพาะที่ Inventory.Add สำเร็จเพื่อแสดงตรงกับที่ได้รับจริง สร้างแถวก่อนเปิดแผงผลลัพธ์ แถวคงอยู่จนกลับ/ออกซีน ไม่ใช้เวลาลบ 3 วินาทีของ pickup HUD ไม่แตะลูกอื่นของ Container ลบเฉพาะ instance ที่ระบบสร้างเมื่อ ExplorationRun ถูกทำลาย ไม่มีการเปลี่ยนสูตรหรือแจกโบนัสเพิ่มจากการแสดงผล Bonus Text เดิมเป็น optional ยังใช้สรุปข้อความได้

ผลตรวจ: runtime ทั้งชุดคอมไพล์กับ Roslyn/references Unity จริงผ่าน ทดสอบยอดรวมชนิดซ้ำ/แยกโบนัส/ข้ามค่าผิดและจำนวนเกิน int ด้วย source จริงผ่าน 10 checks รวมถึงตรวจ scene อ้าง Result Row Prefab ตัวเดียวกับหน้าซ่อม ลบเครื่องมือตรวจชั่วคราวแล้ว ยังไม่ได้เรนเดอร์หรือทดสอบ UI ใน Play Mode ผู้ใช้ต้องสร้าง/ลาก **Materials Container** ของแต่ละผลลัพธ์และสร้าง/ลาก **Bonus Icon** ใน prefab ก่อนแสดงครบ ไม่สร้าง UI Toolkit/Panel/Container/รูปแทนให้

## จบสำรวจ / Game Over — ใช้ Canvas ของผู้ใช้ (2026-10-27)

เพิ่ม `Scripts/ExplorationRun.cs` บน **MainUI ใน SampleScene** ตั้ง Duration=30 และ Return Scene Name=Repair ผูก Ship เป็นผู้เล่นแล้ว ไม่สร้าง Panel/Button/Text หรือ UI Toolkit เพิ่ม ผู้ใช้เป็นคนทำหน้าตาทั้งหมด

เริ่มนับ 30 วินาทีตั้งแต่เริ่มซีนโดยใช้เวลาเกม (พักเกมแล้วเวลาไม่ลด) ถ้ายานตายขึ้น Game Over ถ้ารอดจนครบเวลาขึ้น Exploration Complete จบรอบครั้งเดียว หยุดฉากด้วย Time.timeScale=0 และปิด controller/Enemy/EnemySpawnManager/ShipGun/สคริปต์ IShipProjectile ในซีนเพื่อไม่ให้ Update ยิงหรือเช็กกระสุนต่อขณะหยุด กล้องเดิมหยุดตาม deltaTime=0 ปุ่ม UI ยังกดได้ คืน Time.timeScale=1 เมื่อโหลด Repair และคืนค่าก่อนหยุดถ้า controller ถูกทำลายโดยทางอื่น

Panel เริ่มปิดและเปิดผลลัพธ์ที่ตรงด้วย **SetActive** ไม่มี alpha/fade/แอนิเมชันเสริม ต้องให้ ExplorationRun อยู่ MainUI ที่เปิดเสมอ อย่าย้ายเข้าไปใต้ Panel ผลลัพธ์หรือผูก MainUI เองเป็น Panel

### ช่องที่ผู้ใช้ต้องผูกบน MainUI > ExplorationRun

- **Game Over Panel / Success Panel**: GameObject แผงแพ้และสำเร็จของคุณ
- **Game Over Back Button / Success Back Button**: ปุ่มกลับบนแต่ละแผง ระบบผูก On Click ให้ ไม่ต้องผูกซ้ำ หรือใช้ปุ่มเรียก `ExplorationRun.ReturnToRepair()` เอง
- **Timer Text** (optional): TMP_Text แสดงวินาทีที่เหลือ 30…0
- **Game Over Title Text / Success Title Text** (optional): TMP_Text แสดง `Game Over` / `Exploration Complete`
- **Bonus Text** (optional): TMP_Text หลายบรรทัด แสดง `Bonus Materials` และชื่อ/+จำนวน หรือ `None` หากไม่มีโบนัส
- **Ship** ผูกแล้ว, **Duration** เป็นวินาทีตั้ง 30 แล้ว, **Return Scene Name** ตั้ง Repair แล้ว มีซีนใน Build Profile อยู่เดิม

### วัสดุและโบนัส

เก็บวัสดุที่หาได้ไว้ทั้งตอนแพ้และสำเร็จ ไม่ล้าง/หักคลังตอนแพ้ สำเร็จเท่านั้นได้โบนัสเพิ่ม: จำนวนรวมของ ShipPickup ที่เก็บสำเร็จ **ในรอบนี้ / 10 ปัดลง** แล้วสุ่มชนิดแบบโอกาสเท่ากันจากชนิดที่เก็บได้รอบนี้ทีละหน่วย เช่น เก็บรวม 27 ได้โบนัส 2 ชิ้น ซึ่งอาจเป็นชนิดเดียวกันหรือคนละชนิด ไม่สุ่มชนิดที่ไม่เคยเก็บ ไม่รวมคลังเดิม/ของโหลดจากเซฟ/การเพิ่ม Inventory ทั่วไป/โบนัสเอง หากเก็บน้อยกว่า 10 ได้โบนัส 0

`ExplorationRewards.cs` เก็บยอดรอบนี้แยกจากคลัง ส่วน ExplorationRun จ่ายโบนัสผ่าน Inventory.Add จึงใช้ระบบ JSON เดิม และบันทึกหลังจบ/ก่อนกลับ ป้องกันการจ่ายซ้ำด้วย HasEnded ซึ่งถูกตั้งก่อนแจกโบนัส ไม่ส่ง pickupCollected สำหรับโบนัส จึงไม่ทำแจ้งเตือนเก็บของซ้ำ หากคลังชน int.MaxValue จะเพิ่มได้เฉพาะที่เหลือและแสดงยอดที่เพิ่มจริง กรณียอดรอบเดียวสูงผิดปกติเกิน 21,474,836,470 ชิ้น จำกัดโบนัสรวมไม่เกิน int.MaxValue ตามชนิดจำนวนของ ShipInventory

ปุ่มกลับโหลด Repair หน้าหลักตามการเริ่มซีนเดิม รักษาเงิน/วัสดุ/ชุดยานผ่าน ResourceManager ไม่มีการกดกลับอัตโนมัติ หากไม่ได้ผูก Panel จะมี warning และรอบยังหยุดตามกติกา ดังนั้น **ต้องทำและผูก Panel กับ Back Button ก่อนทดสอบการเล่นครบวงจร**

ผลตรวจ: runtime ทั้งชุดคอมไพล์ด้วย Roslyn/references Unity จริงผ่าน ทดสอบ source ExplorationRewards ผ่าน 19 checks รวม 0/9/10/29/99/100 ชิ้น การรวมชนิดซ้ำ ปัดลง สุ่มเฉพาะชนิดที่เก็บและไม่เปลี่ยนยอดรอบ ตรวจ component/fileID/Ship/เวลา/ซีนกลับแล้ว ลบเครื่องมือตรวจชั่วคราว ไม่ได้ทดสอบ Play Mode การตาย/กดกลับ/หยุดฉากจริงหรือเรนเดอร์ UI เพราะผู้ใช้ยังไม่ได้สร้างแผง

## แจ้งเตือนเก็บของบน Canvas จริง (2026-10-27)

เพิ่มบน `MainUI > ShipHUD` ใน SampleScene ตามผู้ใช้ขอ: ทุกครั้งที่ ShipPickup เพิ่มของเข้าคลังสำเร็จ เสกแถวจาก `UI/pre/RepiarMat.prefab` (ตัวเดียวกับ Inventory Item Prefab หน้าซ่อม) แสดงรูป ชื่อวัสดุ และ `+จำนวน` แยกหนึ่งแถวต่อ pickup แล้วปิด GameObject/ลบเฉพาะแถวนั้นหลัง 3 วินาทีจริง ไม่เฟด ไม่เพิ่มแอนิเมชัน ไม่แก้ layout หรือลูกอื่นใน Container ไม่รวมรายการซ้ำเข้าด้วยกัน

ช่องใหม่บน **ShipHUD** กลุ่ม Successful Pickups:

- **Pickup Container**: RectTransform พื้นที่แจ้งเตือนของผู้ใช้ ยังว่างเพราะผู้ใช้ยังไม่ได้สร้าง ให้สร้าง Container ใน Canvas ตามตำแหน่ง/การจัดแถวที่ต้องการ แล้วลากลงช่องนี้
- **Pickup Row Prefab**: รับ RepairMaterialDragItem บน prefab วัสดุเดิม ผูก RepiarMat.prefab ให้แล้วใน SampleScene ไม่ต้องสร้าง prefab ใหม่
- **Pickup Notification Duration**: เวลาแสดงต่อแถว หน่วยวินาที ค่า 3 ตามผู้ใช้เลือก

ระบบไม่สร้าง Container/กราฟิกสำรอง ถ้ายังไม่ใส่ reference จะแจ้ง warning ครั้งเดียวเมื่อเก็บของและยังเก็บเข้าคลังตามปกติ การปิด ShipHUD/MainUI ล้างเฉพาะ instance แจ้งเตือนที่ระบบสร้าง เมื่อเปิดใหม่รอของที่เก็บครั้งใหม่ ไม่ย้อนแจ้งของเก่า

แก้ `ShipInventory` เพิ่ม event `pickupCollected` และ `ShipPickup.TryCollect` ส่งหลัง Inventory.Add สำเร็จเท่านั้น ไม่ส่งจากการโหลดเซฟ/เพิ่มคลังทั่วไป/ใช้วัสดุซ่อม/เก็บล้มเหลว เก็บซ้ำด้วยหลาย collider ยังคงถูกกันด้วย collected เดิม ShipHUD subscribe เฉพาะคลังของยานที่ติดตามและ unsubscribe เมื่อปิด/เปลี่ยนยาน

เพิ่ม `RepairMaterialDragItem.DisplayPickup` ใช้ช่อง Icon / Material Name / Owned Text เดิมของ prefab เป็นรูป/ชื่อ/+จำนวน โดยไม่ผูก interaction โรงซ่อม จึงลากไปซ่อมไม่ได้ในแถวแจ้งเตือน ถ้า pickup ไม่มี ShipMaterialData จะค้น Material Catalog ด้วย Item ID; ถ้ายังไม่พบ แสดง Item ID และจำนวนโดยไม่มีรูป ไม่สร้าง asset สมมุติ

ผลตรวจ: คอมไพล์ runtime ทั้งชุดด้วย Roslyn/references จริงของ Unity ผ่าน ตรวจ prefab GUID/component fileID ตรง Inventory Item Prefab หน้าซ่อม และช่อง Duration=3 / Container ว่างตามที่ตกลง ยังไม่ได้เรนเดอร์/ทดสอบ Play Mode เพราะยังไม่มี Container ผู้ใช้ต้องสร้างและลาก **Pickup Container** เพียงช่องเดียวก่อนเห็นรายการบนจอ ไม่ต้องแก้ prefab หรือผูก Text/Image ใหม่

สร้างตามคำขอ 2026-10-26 เป็นตัวอย่างที่ปิด/แทนได้ ไม่แก้หรือปิด MainUI และ ShipHUD แบบ UGUI เดิม

**ข้อกำหนดล่าสุด: UI จริงใช้ Canvas/UGUI ไม่ใช่ UI Toolkit** ติด **ShipHUD** ไว้บน **MainUI** ใน SampleScene แล้ว เลือก MainUI เพื่อผูก TMP Text, Slider หรือ Image Filled ของจริง ข้อมูลที่ต้องลากและวิธีส่งรายการมาร์กอยู่ใน SpaceGarage/README.md ส่วน Toolkit ในโฟลเดอร์นี้คงเป็นตัวอย่างที่ปิดได้เท่านั้น

## Canvas ของจริง

- สคริปต์ **ShipHUD** อยู่ใน Add Component เมนู **Space Garage > UI > Canvas Combat HUD** ใช้บน Canvas หรือ panel ที่จัดไว้ได้ ไม่สร้างหน้าตา runtime สำรอง
- MainUI ผูก ShipController ไว้แล้ว เพิ่ม **Auto Find Player** เพื่อค้น Tag Player เมื่อไม่ได้ผูกหรือเปลี่ยนยาน
- ลาก TMP Text ลง Health Text / Boost Text / Speed Text / Dodge Cooldown Text ตามข้อมูลที่จะใช้
- ลาก Slider หรือ Image แบบ Filled ลง Health / Boost / Dodge Ready แต่ละกลุ่ม เลือกใช้เท่าที่ UI จริงมีได้
- แถววัสดุของจริงติด ShipTrackedMaterialRow ผูก Image/TMP ของแถวแล้วทำเป็น Prefab ลากลง **Material Row Prefab** และลาก RectTransform พื้นที่รายการลง **Material Container** บน ShipHUD ระบบจะเสกวัสดุที่มาร์กเข้าไปตาม SetTrackedMaterials และใช้แถวซ้ำ ไม่แตะลูกที่ผู้ใช้จัดไว้ใน Container ถ้าไม่ใส่ Container ยังใช้ array Material Rows แบบเดิมได้
- Container ต้องจัด VerticalLayoutGroup/GridLayoutGroup หรือวิธีจัดตำแหน่งของตัวเองไว้ ระบบไม่สร้างหรือแก้ layout ให้ ไม่สร้างแถวสำรองเมื่อไม่ใส่ Row Prefab
- ตอนนี้ MainUI ในไฟล์ซีนยังไม่มี Text/Slider/Image ลูกให้ผูก จึงเว้นช่องไว้ ไม่สร้างหรือย้าย layout ให้ ผู้ใช้ต้องเตรียมกราฟิกและลากช่องเหล่านี้เอง

## เปิดดูและปิด

วาง `CombatHUD_Example.prefab` ไว้ใน `Scenes/SampleScene.unity` แล้ว เปิดซีนและกด Play เพื่อแสดงข้อมูลจริงจาก PlayerShip

- ปิดตัวอย่างทั้งหมด: ยกเลิก Active ที่ **CombatHUD_Example** ใน Hierarchy หรือปิด component **ShipCombatHUDToolkit**
- ซ่อนชั่วคราว: เอาติ๊ก **Visible** ออก หรือเรียก `SetVisible(false)` คืนด้วย `SetVisible(true)`
- ปิดเฉพาะเป้าเมาส์: เอาติ๊ก **Show Mouse Reticle** ออก
- ลบ GameObject ตัวอย่างได้โดยไม่กระทบยาน ศัตรู กล้อง หรือ UGUI เดิม ถ้าใช้คนละซีนลาก Prefab นี้ลงได้เลย จะค้น Tag Player ถ้าไม่ได้ลาก Ship

## โครงไฟล์

| ไฟล์ | หน้าที่ |
| --- | --- |
| CombatHUD_Example.prefab | GameObject ตัวอย่าง เปิด/ปิดหรือเอาออกจากซีนได้ |
| ../Scripts/ShipCombatHUDToolkit.cs | ผูกข้อมูลยานและรายการมาร์กกับ VisualElement ไม่แก้กติกาเกม |
| Resources/SpaceGarageHUD/CombatHUD.uxml | โครงหน้าจอ เปิดด้วย UI Builder ได้ |
| Resources/SpaceGarageHUD/CombatHUD.uss | สี ขนาด ตำแหน่ง และรูปแบบแถบ/แถววัสดุ |
| Resources/SpaceGarageHUD/CombatHUDTheme.tss | นำเข้า default runtime theme ของ Unity เพื่อให้ Label มีฟอนต์มาตรฐาน |
| ../Scripts/ShipMaterialRequirement.cs | ข้อมูลวัสดุที่แชร์กับ ShipHUD เดิม แยกไฟล์เพื่อไม่ผูก Toolkit กับ component UGUI |

หน้าตาใช้แผงเข้มโปร่งใส สีฟ้า/เขียว/ส้ม ข้อมูลอยู่ขอบจอ: สถานะบนซ้าย วัสดุมาร์กด้านขวา เลือด/บูสต์ล่างซ้าย ความเร็ว/ความพร้อมหลบล่างขวา เป้าเล็งตามเมาส์ และป้ายปุ่มควบคุม ไม่เปลี่ยน cursor ของระบบ ไม่แย่งการคลิกหรือโฟกัสเกม

## ปรับหรือเปลี่ยน UXML ของตัวอย่าง Toolkit

1. สร้างหรือทำสำเนา UXML/USS ไปแก้ใน UI Builder
2. ลาก UXML ใหม่ลงช่อง **Layout** บน ShipCombatHUDToolkit แล้วเริ่ม Play ใหม่
3. ถ้ามี PanelSettings ของจริง ลากลง **Panel Settings** ได้ ถ้าเว้นว่างตัวอย่างสร้าง panel ในหน่วยความจำแบบ Scale With Screen Size อ้างอิง 1920×1080, Match 0.5 และ sorting order 100 ให้ โดยทำลาย panel เมื่อเอา component ออก
4. คงชื่อ element ที่ต้องการผูกข้อมูลตามตารางด้านล่าง ชื่อที่ไม่อยู่ใน UXML จะถูกข้าม ไม่สร้าง widget สำรองแทน
5. ถ้าจะใช้สคริปต์ของจริงเอง ให้ปิด/เอา GameObject ตัวอย่างออกแล้วใช้ HUD ของคุณได้เลย ไม่ต้องแก้ระบบยาน

| ชื่อใน UXML | ชนิด | ข้อมูล |
| --- | --- | --- |
| combat-hud | VisualElement | root สำหรับคลาส boosting/low-health |
| health-value | Label | HP ปัจจุบัน / สูงสุด |
| health-fill | VisualElement | เติม width 0–100% ตามเลือด |
| boost-value | Label | เกจบูสต์ปัจจุบัน / สูงสุด |
| boost-fill | VisualElement | เติมตามเกจ Shift boost |
| speed-value | Label | ความเร็วบินปัจจุบัน 1 ตำแหน่งทศนิยม |
| dodge-value | Label | READY / ROLLING / เวลาคูลดาวน์ |
| dodge-fill | VisualElement | ความพร้อมท่าหลบ 0–100% |
| mode-status | Label | สถานะ CRUISE / BOOST / EVADE / OFFLINE |
| mouse-reticle | VisualElement | เป้าเล็ง 30×30 ตามเมาส์ ปิดได้จาก Inspector |
| materials-list | VisualElement | container รับแถววัสดุที่ระบบมาร์กส่งมา |
| materials-empty | Label | ข้อความเมื่อไม่มีรายการมาร์ก |

แถววัสดุสร้างจากข้อมูลภายใน materials-list ใช้คลาส USS `material-row`, `material-icon`, `material-name`, `material-count`, `material-complete` ถ้าหน้าตาจริงเปลี่ยนโครงของแถวมากกว่านี้ ให้แก้ RebuildMaterialRows หรือใช้สคริปต์ UI ของคุณเอง ตัวอย่างนี้เป็น display-only จึงตั้ง PickingMode.Ignore และไม่รับคลิกปุ่มใน UXML

## เชื่อมข้อมูล

- **Ship**: ลาก ShipController ของยานผู้เล่น ตัวอย่างใน SampleScene ผูกไว้แล้ว
- `SetShip(controller)` เปลี่ยนยานที่ติดตามได้
- ระบบมาร์กภายนอกส่ง `SetTrackedMaterials(IEnumerable<ShipMaterialRequirement>)` เหมือน API ของ ShipHUD เดิม โดยส่ง asset ShipMaterialData และ requiredAmount
- HUD อ่านรูป/ชื่อจาก asset และจำนวนที่มีจาก ShipInventory อัปเดตเมื่อคลังเปลี่ยน ไม่สร้างรายการมาร์กเอง ไม่ใส่ตัวอย่างวัสดุปลอม และไม่อ่านรายการจาก component ShipHUD เดิมอัตโนมัติ ถ้าจะแสดงทั้งสอง HUD ให้ระบบมาร์กส่งให้ทั้งสองตัว
- `SetTrackedMaterials(null)` ล้างรายการ; เมื่อยังไม่มีรายการจะแสดง No materials marked

## ผลตรวจและข้อจำกัด

- คอมไพล์กับ DLL จริงของ Unity 6000.3.8f1 / Input System / UIElements / Cinemachine ผ่าน ไม่มี error/warning
- UXML ผ่าน XML parser และตรวจชื่อผูกครบ 12 จุด ตำแหน่งไฟล์ USS/TSS และ GUID ของ Prefab/สคริปต์/ยานในซีนตรวจแล้ว
- แก้รายงาน Toolkit ไม่ขึ้น: เปลี่ยนจาก CloneTree แทรกเองไปใช้ UIDocument.visualTreeAsset ซึ่งให้ Unity จัด lifecycle ของต้นไม้ UI กำหนด root เต็มจอชัดเจน และผูกข้อมูลใหม่เมื่อ UIDocument เปลี่ยน root ไม่ปล่อย reference ชี้ต้นไม้เก่าที่ไม่อยู่บน panel
- ตรวจใน Play Mode จริงผ่านสคริปต์ Editor ชั่วคราวและดู PNG จาก ScreenCapture แล้ว: พบ HUD/UIDocument/panel/UXML/theme ครบ root และ combat-hud มีขนาด 1920×1080 ภาพแสดง HP 100/100, Boost 100/100, Speed และ Dodge READY พร้อมแผงวัสดุว่าง ตัวอย่างขึ้นจริงหลังแก้ ไม่ได้อ้างผลนี้จากคอมไพล์อย่างเดียว
- การตรวจภาพนี้ไม่ได้ทดสอบการกดบูสต์/Dodge หรือ Canvas กราฟิกจริงที่ยังไม่ได้ผูก สคริปต์ตรวจและภาพชั่วคราวลบหลังตรวจตามกติกาโปรเจกต์ ไม่ทิ้งระบบอัตโนมัติสั่ง Play ไว้
- HUD แสดงเมื่อ Play; ใช้ UI Builder เปิด UXML เพื่อแก้และดูตัวอย่างใน Editor ไม่มีการสร้าง UI ค้างใน Edit Mode
- ข้อความตัวอย่างเป็นอังกฤษและใช้ default runtime theme ถ้าข้อความวัสดุภาษาไทยแสดงไม่ครบ ต้องเตรียม PanelSettings/TextSettings และฟอนต์ไทยของจริงเอง
- ไม่มีการย้าย ลบ ซ่อน หรือเปลี่ยนขนาด MainUI/Canvas/UGUI เดิม ผู้ใช้เลือกปิดชุดที่ไม่ต้องการเองได้

เสียง UI ใช้ UISfxManager เดิมที่ปรับให้ไม่มี dependency GameSfx: Auto Bind Scene Buttons ผูก click/hover โดยไม่เปลี่ยนภาพหรือ layout, CustomizationUI ผูกแถวที่สร้างใหม่ด้วย ผู้ใช้ต้องใส่ manager และคลิป Click/Hover เอง รายละเอียด Scirpt/Tutorial/README.md (2026-10-27)

## Start / Settings ของผู้ใช้ (2026-10-27)

ใช้ Canvas/UGUI ที่ผู้ใช้สร้างเอง ไม่มีการสร้างหรือจัดหน้าตา UI/Canvas/แอนิเมชัน สคริปต์เปิดปิดด้วย SetActive ใส่ **StartMenuUI** บน GameObject ที่ active ตลอดและอยู่นอก/ไม่เป็นลูกของแผงที่จะปิด:

- **Start Panel**: GameObject หน้าแรกที่มี Start, Settings, Exit
- **Settings Panel**: GameObject หน้าตั้งค่า
- **Main UI**: root หน้าหลักเดิม ต้องแยกจาก Start/Settings ห้ามลาก Canvas แม่ที่ครอบทั้งสามหน้า
- **Repair UI**: component RepairUI เดิม ถ้าว่างค้นหาในซีนรวม inactive ใช้ SetMainMenuVisible กัน Refresh เปิดเมนูทับ Start
- **Start Button / Settings Button / Exit Button / Settings Back Button**: Button ของผู้ใช้ สคริปต์ผูก listener เอง ไม่ต้องเพิ่ม On Click ซ้ำ

Start เรียก ResourceManager.BeginGame แล้วเปิด Main UI; Settings เปิดหน้าตั้งค่า; Back บันทึกค่าและกลับหน้าก่อน; Exit บันทึกแล้ว Application.Quit (Editor ไม่หยุด Play Mode) เมื่อกลับ Repair จากต่อสู้ภายในรอบเปิดแอปเดียวกันจะเข้า Main UI โดยตรง เปิดแอปใหม่แสดง Start อีกครั้งแต่ใช้เซฟเดิม

ใส่ **SettingsUI** บนแผง Settings ของผู้ใช้:

- **Master Volume / Music Volume / Sfx Volume**: Slider เสียงรวม/เพลง/เอฟเฟกต์ (UI/ปืน/ไอพ่น) ช่วง 0–1 สคริปต์กำหนดช่วงให้ เปลี่ยนเสียงทันที ใส่เฉพาะช่องที่ต้องการแสดงได้
- **Resolution Dropdown**: TMP_Dropdown เติมขนาดจอที่รองรับ ตัดขนาดซ้ำ รวมขนาดปัจจุบัน/ที่เซฟไว้
- **Fullscreen Toggle**: เปิด=FullScreenWindow ปิด=Windowed
- **Apply Display Button**: Button ใช้ความละเอียดและโหมดจอที่เลือก ต้องกด Apply จึงเปลี่ยนจอ

GarageSettings เก็บค่าผ่าน PlayerPrefs กลุ่ม SpaceGarage.Settings.* แยกจากเซฟเกม โหลดก่อนเข้าซีน Master ใช้ AudioListener.volume; เพลง/เอฟเฟกต์คูณค่าบน UISfxManager เดิม ไม่ล้างคลิปหรือ gain ที่ใส่ไว้ ต้องมี UISfxManager พร้อมคลิปหากต้องการเสียง ไม่สร้าง manager สำรอง

การเปลี่ยนขนาด/โหมดจอมีผลใน Build เท่านั้น Editor บันทึกค่าแต่ไม่เปลี่ยน Game View หรือเลย์เอาต์ ผู้ใช้ต้องสร้าง/จัดฟอนต์/ข้อความภาษาอังกฤษและลาก reference ข้างต้นเอง ยังไม่ได้ผูก StartMenuUI/SettingsUI ในซีนเพราะผู้ใช้จะทำ UI เอง

ผลตรวจ: คอมไพล์ Assembly-CSharp ทั้งชุดกับ Roslyn/reference Unity จริงผ่าน exit 0 ตรวจเส้นทางเริ่มครั้งแรก/เซฟเดิม/เซฟเสีย/กลับ Repair และถอด listener แล้ว ยังไม่ได้คลิก UI ฟังเสียง หรือเปลี่ยนจอใน Build ไม่ได้แตะเซฟจริง

### ช่อง Repair UI รับแผงที่แยกจากโค้ด (2026-10-27)

StartMenuUI > Repair UI เปลี่ยนจากบังคับ RepairUI component เป็น UnityEngine.Object เพื่อรับทั้ง GameObject แผง UI และ component เดิมโดยเก็บชื่อ field/reference เดิมไว้ โค้ด RepairUI กับ panel อยู่คนละที่ได้: ใช้ component ที่ลากโดยตรง หรือค้นหาใน GameObject ที่ลากรวมลูก inactive; ถ้าไม่มีจะค้นหา RepairUI แยกในซีนรวม inactive แล้วเรียก SetMainMenuVisible ตามเดิม ไม่ย้ายวัตถุ ไม่เปิด panel หน้าซ่อมตอนกด Start และไม่เปลี่ยน SetActive/layout เดิม ไม่มีช่องใหม่ ผู้ใช้ลาก panel ที่มีอยู่หรือ component เดิมลง Repair UI ได้ ไม่ต้องย้ายโค้ดมาอยู่บน panel

Start/New Game ล่าสุด (2026-10-27): ปุ่ม Start เรียก newGame.TryInitializeOnce ร่วมกับ On Start แทน BeginGame ตรวจ PlayerPrefs SpaceGarage.NewGame.Completed แล้วเริ่มชุดใหม่หนึ่งครั้งแม้มี JSON เก่า หลังสำเร็จบันทึก flag ไม่ทำซ้ำ ข้อมูลเต็มและช่อง Completed อยู่ Customization/README.md ไม่เปลี่ยน panel/animation/reference

Tutorial (2026-10-27): ผู้ใช้อนุญาตสร้างกล่องง่าย ๆ แล้ว เพิ่ม SpaceGarage/UI/FirstRepairTutorialDialog.prefab ใต้ Canvas MainUi เดิมผ่าน TutorialManager บน root FirstRepairTutorial ในซีน Repair กล่องบนกลางพื้นเข้ม Title/Instructions/Counter ไม่มี dim/animation/Next/Skip ไม่ย้าย UI เดิม แสดงหลัง Start เปิด Main Menu สอน Repair/ซ่อมหนึ่งชิ้น/Back/Explore ครั้งเดียวเมื่อจบบท รายละเอียดและผลตรวจใน Scirpt/Tutorial/README.md ผูก reference ให้ครบแล้ว ไม่ต้องสร้างกล่องเอง
