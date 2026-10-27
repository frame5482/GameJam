# ระบบควบคุมยานต้นแบบ

## Rail Gun: จุดเริ่มขยายและพื้นที่ตรวจโดน (2026-10-27)

ตรวจ prefab จริงที่ผู้ใช้ย้ายไว้ `Model/GUN/Rail Gun.prefab` (Part ID/GUID และ Muzzle เดิมคงเดิม) และรันทดสอบจริงใน Unity Editor ด้วย prefab Enemy/Physics/ShipGun/Enemy.TakeHit ในซีนทดสอบแยก ไม่แตะเซฟหรือศัตรูของผู้ใช้: โค้ดเดิมทำ HP ลด 10 ที่ระยะ 10/40 หน่วย, ไม่โดนที่ 60, กรณี origin อยู่ใน hitbox โดน และกันโดนซ้ำได้ จึงไม่ได้สรุปว่าดาเมจถูกปิดทุกกรณี ปัญหาภาพกับการชนที่พบคือกราฟใช้ quad pivot กลางแผ่นจึงขยายย้อนกลับด้วย ขณะที่ตรวจโดนเป็น ray ไม่มีความหนาและยิงไปข้างหน้าจาก Muzzle เท่านั้น

แก้เฉพาะจุดต่อใน `VFX/JET/BulletBeam/CataclysmBeam.vfx`: เพิ่ม exposed float **Beam Pivot Y** ค่าเริ่มต้น 0 และ Set Pivot.Y ใน Output ทั้งสาม ไม่ลบโหนด/profile/สี/ขอบ/อัตราเกิดเดิม ShipBeamEmitter ตั้งค่า -0.5 เฉพาะ Rail เพื่อให้ขอบต้น quad ยึดที่จุดเกิดและขยายไปตาม velocity ด้านหน้า ส่วน Laser ส่ง 0 จึงคงภาพเดิม ตั้งตำแหน่ง/rotation ของ root VFX ให้ตรง Muzzle **ก่อน**เปิด/Play ครั้งแรกด้วย ไม่ย้าย Muzzle หรือโมเดลปืนของผู้ใช้

เพิ่มช่อง **ShipGun > Rail Damage Radius** และ **Laser Damage Radius** ค่า **3 หน่วย** ทั้งสองชนิดตามที่ผู้ใช้ย้ำให้ hitbox ใหญ่ขึ้นมาก (เส้นผ่านศูนย์กลางประมาณ 6 หน่วย ตั้งใน Model/GUN/Rail Gun.prefab และ Lascer.prefab แล้ว) ค่า 0 ใช้ ray บางเหมือนเดิม Rail ตรวจชนเป็น sphere sweep ที่หนาขึ้นตามช่วงขยาย 0→50 ส่วน Laser ใช้รัศมีเต็มตลอดการกดค้าง ตรวจกรณีต้นลำแสงซ้อน collider ก่อน จำกัดปลายระยะไว้ Range เดิม (Rail 50 / Laser ใน prefab 20) หยุดที่ collider สิ่งกีดขวางแรก ใช้ marker/ข้ามเจ้าของ/กระสุนเดิม ไม่ทะลุกำแพง ไม่เล็งเมาส์ Rail ยังโดนเป้าหมายละหนึ่งครั้งต่อพัลส์รวมทั้งสองปาก ตัวที่เข้ามาระหว่างแสดงยังโดนได้ Laser ทำดาเมจซ้ำตาม interval เดิม ไม่เปลี่ยนดาเมจ/Def/โล่หรือรอบยิง

ไฟล์สคริปต์ที่แก้: ShipGun.cs และ ShipBeamEmitter.cs ไม่มี UI ใหม่ ไม่มี reference ใหม่ให้ลาก เพิ่ม Rail Damage Radius / Laser Damage Radius ดังกล่าว ส่วน Beam Pivot Y เป็น property กราฟที่ปืนสั่งอัตโนมัติ ผู้ใช้ไม่ต้องแก้ตำแหน่งหรือกรอกค่าต่อ เริ่ม Play ใหม่เพื่อโหลดกราฟ/โค้ดที่แก้

ผลตรวจรอบแก้: ทดสอบ API จริงใน Unity 6000.3.8f1 **Edit Mode** ด้วย prefab Rail Gun/Enemy และ Physics ในซีนชั่วคราวแยก ไม่แตะเซฟ ผ่านดาเมจที่10/40, ไม่โดนที่60, origin-inside, กันโดนซ้ำต่อพัลส์, เป้าเยื้องข้าง3.3หน่วย rayเดิมไม่โดนแต่ sweepรัศมี1โดน10, กำแพงบังได้, VisualEffect มี Beam Pivot Y=-0.5 สำหรับRail/0สำหรับLaser, Sizeทั้งคู่25และrootตรงMuzzle หลังผู้ใช้ขอรัศมีใหญ่เพิ่มเป็น3ทั้งสองปืน ตรวจ source SpaceGarage ล่าสุดด้วย Roslyn/references Unity ผ่าน และ prefab มีค่า3ครบ แต่ **การรัน Unity ชุดสุดท้ายรัศมี3ถูกขวางด้วย compile errors ของ Scirpt/Tutorial (GlobalQuestState/UISfxManager/InteractableObject ไม่พบ)** ผู้ใช้ยืนยันกำลังแก้จากอีกงาน จึงไม่แก้ไฟล์เหล่านั้นแทน ไม่อ้างว่าทดสอบ Play Mode/ภาพเฟรมจริงผ่านแล้ว ลบสคริปต์ตรวจ/ซีน/ไฟล์ผลชั่วคราวแล้ว ผลสำคัญเก็บในหัวข้อนี้

## Rail Gun กลับมายิงตรง ไม่ตามเมาส์ (2026-10-27)

ตามผู้ใช้ล่าสุด แก้ ShipGun ให้ Rail Gun ใช้ forward (+Z) และ rotation ของ Muzzle แต่ละจุดทั้งภาพลำแสงและ ray ดาเมจ ถ้าไม่มี Muzzle ใช้ transform ของ ShipGun ไม่ต้องผ่านกล้อง/ตำแหน่งเมาส์/viewport หรือจุดเป้า AI เพื่อลั่นกระสุน ยานยังหันด้วยระบบบินเดิม ลบ ShipBeamAim.cs/.meta ที่ไม่มีการใช้งานหลังตรวจ reference แล้ว ไม่แก้ UI หรือแอนิเมชัน

คง Size/Size 1 จาก 0→50 ใน 0.5s, Rail Range=50, สีฟ้า/ช่องสีเดิม, แสดง1s/พัก1s และตรวจโดนตลอดช่วงแสงโดยแต่ละเป้าหมายรับได้ครั้งเดียวต่อพัลส์รวมสองปาก ตัวที่เข้ามาทีหลังยังโดนได้ ไม่มีช่องใหม่ ไม่ต้องตั้งค่าเพิ่ม คอมไพล์ runtime กับ Roslyn/references Unity จริงผ่าน ตรวจว่า Rail ไม่เรียก TryGetAimPoint และภาพ/ดาเมจใช้ทิศเดียวกันแล้ว ยังไม่ได้ทดสอบยิง/ภาพจริงใน Play Mode

## ศัตรูสูงสุด 3 ลำ (2026-10-27)

ตั้ง EnemySpawnManager ค่าเริ่มต้นในสคริปต์และ `Examples/Enemies/EnemySpawnManager.prefab > Max Alive` จาก 5 เป็น **3** SampleScene ใช้ prefab นี้โดยไม่มี override เมื่อนับครบ 3 จะไม่เสกเพิ่ม และ Prune เดิมตัดศัตรูที่ตาย/ถูกทำลายก่อนตรวจเพดานจึงเกิดทดแทนได้เมื่อมีช่องว่าง ใช้ Spawn Interval เดิม 5 วินาที เสกครั้งละหนึ่งตัว ไม่ได้เสกสามตัวพร้อมกันหรือเกิดทันทีใน callback ตาย ไม่เปลี่ยนพูล/ตำแหน่งสุ่ม/ดาเมจ/AI/UI

ไม่มีช่องใหม่ ไม่ต้องตั้งค่าเพิ่ม ให้เริ่ม Play ใหม่เพื่อใช้เพดานใหม่ ไม่ลบศัตรูที่มีชีวิตกลางรอบเล่นแทนผู้ใช้ ตรวจ Max Alive=3 ใน prefab/สคริปต์, reference SampleScene และลำดับ Prune ก่อน cap แล้ว คอมไพล์ runtime กับ Roslyn/references Unity จริงผ่าน ยังไม่ได้ทดสอบจำนวนศัตรู/ตายแล้วเกิดแทนใน Play Mode

## Rail Gun เล็งเมาส์ด้านหน้าและขยาย Size (2026-10-27)

ตามผู้ใช้ยืนยันล่าสุด Rail Gun เล็งตามจุดใต้เมาส์จากกล้องจริงได้เฉพาะด้านหน้า ไม่เกิน 90 องศาจาก forward ของ root ยาน (รวม pitch) เมาส์เลยหลังจะจำกัดที่ขอบด้านหน้า; จุดตรงหลังพอดีที่ไม่มีทิศด้านข้างจะใช้ forward ยาน กรณี AI ใช้จุดเป้าหมายของ Enemy ภายใต้ขอบเขตเดียวกัน ลำแสงและ ray ดาเมจใช้ BeamDirection เดียวกันของแต่ละ Muzzle ไม่หมุนโมเดลปืนหรือ root ยานตามเมาส์เอง Laser ยังคงยิงตรงตาม Muzzle เดิม

Rail Size ค่าเริ่มต้น **50**, เพิ่มช่องใหม่ **Rail Grow Duration = 0.5s** บน **ShipGun > Laser / Rail Gun** คุมการขยาย **Size และ Size 1** จาก 0 → 50 แบบเชิงเส้นใน 0.5 วินาทีแรก แล้วคงถึงจบพัลส์ 1s ก่อนเป็น 0 และพัก 1s รอบใหม่เริ่มได้ที่ 2s เมื่อกดค้าง ปล่อยระหว่างพัลส์ยังแสดงจนจบตามเดิม เปลี่ยนค่าเริ่มต้น **Rail Range เป็น 50 หน่วย** ตามผู้ใช้ยืนยัน แยกจากขนาดภาพตามเดิม

แก้การตรวจดาเมจจากครั้งเดียวตอนเริ่ม เป็นตรวจทุกเฟรมที่แสงยังอยู่ แต่ศัตรู/เป้าหมายแต่ละตัวโดนได้ **ครั้งเดียวต่อหนึ่งพัลส์ต่อ ShipGun** (รวมสองปาก ไม่ซ้ำจาก collider/ปากอีกฝั่ง) ตัวที่เข้ามาในแนวลำแสงภายหลังยังโดนได้ ล้างชุดเป้าที่โดนเมื่อเริ่มพัลส์ใหม่และเมื่อปิดปืน ยังคง Attack × Damage Multiplier / Def / โล่ / Hit Feedback / ข้ามเจ้าของและกระสุน / หยุดที่สิ่งกีดขวางแรก / ตรวจ origin-inside เดิม ไม่เพิ่มการทะลุเป้าหรือความหนาของ ray

แก้ ShipGun.cs, ShipBeamEmitter.cs (รับ rotation ของภาพ), ShipWeaponCycle.cs (บันทึก Pulse Started At), เพิ่ม ShipBeamAim.cs สำหรับจำกัดทิศ ไม่มีแก้ scene/prefab/VFX graph หรือย้าย reference ปืนเดิม ค่าตั้งต้นพร้อมใช้ ปรับ Rail Size / Rail Grow Duration / Rail Range และสี HDR บน ShipGun ของ prefab ปืนได้ แล้วบันทึก prefab หากมีค่า Rail Size/Range เก่าที่ serialize ไว้ใน Inspector แล้วต้องใช้ค่า 50 ตามรายการใหม่; ตรวจไฟล์ปัจจุบันยังไม่พบ override ของช่องเหล่านี้

ผลตรวจ: runtime ทั้งชุดคอมไพล์ด้วย Roslyn/references Unity จริงผ่าน ทดสอบ source ShipBeamAim/ShipWeaponCycle กับ Vector3/Mathf จริงของ Unity ภายนอก Editor ผ่าน **215 checks** ครอบคลุมทิศหน้า/หลัง/ด้านข้าง/ก้มเงย/ยานหันคนละแกน/ทิศศูนย์, สุ่มทิศ 100 กรณีไม่เลย 90 องศา, พัลส์1s/พัก1s และ Size=25 ที่0.25s/50ที่0.5s ลบเครื่องมือชั่วคราวแล้ว ยังไม่ได้เรนเดอร์ภาพหรือทดสอบยิงทำดาเมจจริงใน Play Mode จึงไม่อ้างว่ายิงในเกมผ่านแล้ว

## บูสต์แล้วควบคุมเลี้ยว/ก้มเงยไวขึ้นมาก (2026-10-27)

แก้ ShipController เฉพาะขณะ IsBoosting=true (กด Shift และมีเกจใช้จริง): เพิ่มความเร็วเลี้ยว/ก้มเงย/เอียงตัว 2 เท่า และลดเวลาหน่วง steering ครึ่งหนึ่ง เช่น Base Wing จาก turn 54 เป็น 108 องศา/s, pitch 36 เป็น 72 องศา/s, bank 75 เป็น 150 องศา/s และ response 0.32 เป็น 0.16s ตัวคูณ Wing/Unity เดิมยังมีผลก่อนเพิ่มความไวบูสต์ มุม pitch ยังคง ±30 องศาและ Bank Angle 25 องศา ไม่เพิ่มการตีลังกา ไม่แก้ความเร็ว/อัตราใช้เกจ/VFX/กล้อง/ดาเมจหรือศัตรู

ช่องใหม่บน **ShipController > Boost Steering Assistance**: **Boost Steering Multiplier** ค่า 2 คูณอัตราเลี้ยว/ก้มเงย/เอียงขณะบูสต์; **Boost Steering Response Multiplier** ค่า 0.5 คูณเวลาหน่วง (ค่าน้อยตอบสนองไวขึ้น) ตั้งค่าใน StarterShip.prefab และ HeavyShip 2/StarterShip.prefab แล้ว ไม่ต้องลาก reference/กรอกเพิ่ม ปล่อย Shift/เกจหมดจะกลับใช้เป้าหมายอัตราปกติผ่าน smoothing เดิม และไม่เปลี่ยนท่า Dodge

คอมไพล์ runtime กับ Roslyn/references Unity จริงผ่าน ตรวจค่า prefab และเส้นทาง IsBoosting/คืนค่าปกติแล้ว ยังไม่ได้ลองบินจริงใน Play Mode ให้เริ่ม Play ใหม่เพื่อใช้โค้ด/ค่าที่แก้

## ตรวจ Laser หลังโดนครั้งแรก/ปากกระบอกเข้า hitbox (2026-10-27)

ผู้ใช้แจ้งว่าเดิมโดนครั้งแรกแล้วกดค้างไม่ลดต่อ ตรวจ ShipGun พบลูป interval ไม่ได้หยุดหลังครั้งแรก และ Enemy.TakeHit ไม่มีช่วงอมตะจาก Hit Reaction แต่พบช่องโหว่ของ Physics.RaycastAll ที่ไม่รายงาน collider ซึ่งครอบ origin ของ ray อยู่ เมื่อยานบินต่อแล้วปากกระบอกเข้า hitbox อาจทำให้การตรวจครั้งถัดไปข้ามศัตรู แก้ DamageBeam ให้ตรวจ OverlapSphere ขนาด 0.001 ที่ปากกระบอกและ ClosestPoint ก่อน ray เพื่อรับกรณีอยู่ข้างใน ใช้ CanAimAt เดิมข้ามเจ้าของ/กระสุน/Trigger อื่น หากพบ collider ที่รับได้ตรง origin จะหยุดที่นั้นเช่นเดียวกับการชนระยะ 0 ไม่ทะลุสิ่งกีดขวาง ไม่เพิ่มความหนาหรือระยะลำแสง

แยก ApplyBeamDamage ให้ overlap/ray ใช้การส่งดาเมจเดียวกัน หนึ่งครั้งต่อปากต่อ tick แม้ collider ซ้อนกัน กดค้างยังตรวจซ้ำทุก Laser Damage Interval เดิม เริ่มโดนทันที ไม่มีพัก Rail ใช้ helper เดียวกันจึงรับ origin-inside ด้วยโดยยังโดนครั้งเดียวต่อพัลส์ ไม่แก้ prefab/scene/VFX/ดาเมจ/ระยะ/Def/Hit Reaction และไม่มีช่องใหม่

คอมไพล์ runtime ทั้งชุดด้วย Roslyn/references Unity จริงผ่าน ตรวจเส้นทางปล่อย/กดค้าง/overlap/ray/ข้ามเจ้าของแล้ว ยังไม่ได้ทดสอบถือ Laser ยิงศัตรูจริงใน Play Mode จึงยังยืนยันไม่ได้ว่ากรณี origin-inside นี้เป็นสาเหตุเดียวกับที่ผู้ใช้พบ ไม่ต้องตั้งค่าเพิ่ม เริ่ม Play ใหม่เพื่อใช้โค้ด หากภาพลำแสงถึงแต่ ray ไปไม่ถึง การตรวจยังใช้ Laser Range เดิม (เริ่มต้น 10 หน่วย) แยกจากขนาด VFX ตามที่ตั้งไว้เดิม

## Laser ทำดาเมจตลอดการกดค้าง (2026-10-27)

ยืนยันตามผู้ใช้: โหมด Laser ไม่มีพัลส์หรือเวลาพักแบบ Rail Gun ทำดาเมจซ้ำตลอดเวลาที่กดค้าง แก้ ShipGun.cs ให้ยิงตรวจดาเมจทันทีเมื่อเริ่มกด แล้วตรวจซ้ำทุก Laser Damage Interval เดิม (ค่าเริ่มต้น 0.1s) จนปล่อย แต่ละครั้ง/ปากใช้ Attack × Damage Multiplier ผ่าน Def/โล่/Hit Feedback ตามเดิม ไม่เปลี่ยนเป็น DPS ต่อเฟรมหรือเพิ่มตัวคูณดาเมจ เมื่อปล่อย/ปิดปืน/เปลี่ยนชนิดปืนจะรีเซ็ตสถานะเริ่มยิง และไม่ทำดาเมจ Laser เมื่อ Time.deltaTime=0 โหมดยิงตรงตาม +Z ของ Muzzle, Range/Size/สีและ Rail เดิมไม่เปลี่ยน

ไม่มีช่อง Inspector ใหม่ ไม่ต้องตั้งค่าเพิ่ม หากต้องการปรับความถี่ใช้ Laser Damage Interval บน ShipGun ช่องเดิม คอมไพล์ runtime ด้วย Roslyn/references Unity จริงผ่าน ยังไม่ได้ทดสอบกดค้าง/ดาเมจจริงใน Play Mode ให้เริ่ม Play ใหม่เพื่อใช้โค้ด

## สี Laser / Rail Gun แยกบนปืน (2026-10-27)

เพิ่มช่อง HDR Color บน **ShipGun > Laser / Rail Gun** ของ Gun Part prefab แต่ละตัว: **Laser Color / Laser Secondary Color** คุม exposed `Color` / `Color 1` ของลำแสงเมื่อ Weapon Type=Laser; **Rail Color / Rail Secondary Color** คุมสอง property เดียวกันเมื่อ Weapon Type=Rail Gun ตั้งสี Laser เริ่มต้นตามค่ากราฟ CataclysmBeam เดิมทั้งสองชั้น (89.4541,5.474129,0,1) และ (0,117.62558,109.94738,1) ตามที่ผู้ใช้ขอ ส่วน Rail ทั้งสองชั้นเริ่มสีฟ้า HDR (0,45,110,1) ปรับ hue/Intensity/alpha ได้ใน color picker บนปืนแล้วบันทึก prefab

แก้ Scripts/ShipGun.cs เลือกสีตามชนิดปืน ส่งทั้งสองปากผ่าน ShipBeamEmitter.cs เขียนสีเฉพาะ VisualEffect instance ของปืนก่อน Play ครั้งแรกและตอนแสดงแต่ละเฟรม ไม่แก้กราฟ/Material/reference กระสุน/ปากกระบอกหรือสีปืนตัวอื่น ใช้ได้กับทั้งปืนผู้เล่นและศัตรูที่ประกอบจาก prefab นั้น ช่วงเปิดปิด Size/Size 1, ทิศยิงตรง, ระยะ ดาเมจ และรอบพัลส์เดิมทั้งหมด หากเปลี่ยน Cataclysm Beam เป็นกราฟอื่น ต้องมี exposed Color/Color 1 ชนิด Color/Vector4 จึงรับค่าสีได้; ไม่มี property จะข้ามเฉพาะค่าสีนั้น

ผลตรวจ: runtime ทั้งชุดคอมไพล์ด้วย Roslyn/references Unity จริงผ่าน ตรวจ graph มี Color/Color 1 ชนิด Color และ default Laser ตรงกราฟเดิม ไม่ได้แก้ไฟล์กราฟ/prefab/scene ไม่ต้องลาก reference เพิ่ม สีเริ่มต้นพร้อมใช้ หากต้องการสีอื่นเลือกบน ShipGun ของ prefab ปืนนั้น ยังไม่ได้ตรวจภาพแสง/Bloom/สีจริงใน Play Mode

## ลดดาเมจศัตรู 50% (2026-10-27)

แก้ `Scripts/Enemy.cs` ตอนส่งค่าปืนผ่าน SetAITarget ให้ใช้ Attack รวมของศัตรู × 0.5 ก่อนตัวคูณปืน ครอบคลุมกระสุน/เลเซอร์/เรล/กระสุนพิเศษที่ผ่าน ShipGun เดิม ไม่แก้ ShipPartStats, prefab ปืนร่วม หรือดาเมจผู้เล่น ความถี่ยิงยังเดิม ดาเมจนี้เป็นค่าก่อนหัก Def/โล่ จึงไม่จำเป็นว่า HP ที่เสียจะเท่าครึ่งหนึ่งพอดีเมื่อมี Def ไม่มีช่องใหม่ ไม่ต้องตั้งค่าเพิ่ม เริ่ม Play ใหม่เพื่อโหลดโค้ด คอมไพล์ runtime ด้วย Roslyn/references Unity จริงผ่าน ยังไม่ได้ทดสอบดาเมจจริงใน Play Mode

## ขยายพื้นที่ยิงโดนศัตรู (2026-10-27)

ตามผู้ใช้ขอให้ยิงโดนง่ายขึ้น ปรับเฉพาะ `Examples/Enemies/EnemyShip.prefab > DamageHitbox > BoxCollider > Size` จาก (4.2,1.8,4.5) เป็น **(5.6,2.6,5.6)** หน่วย คงตำแหน่งลูก (0,0,0.2), Is Trigger=true และ ShipDamageHitbox เดิม ช่วยให้กระสุน/เลเซอร์/เรลที่รองรับ hitbox เดิมยิงเฉียดด้านข้าง/บนล่างได้ง่ายขึ้น ไม่เพิ่มการดูดเป้าหรือกระสุนติดตาม ไม่เปลี่ยนความแม่นยำ/ดาเมจ/รัศมีกระสุนของผู้เล่นหรือศัตรู Collider ชนจริงบน root ยังคง (2.8,0.8,3.25)

ไม่มีช่องใหม่ หากต้องการปรับต่อใช้ BoxCollider > Size ของลูก DamageHitbox เดิม EnemySpawnManager ที่ SampleScene ใช้ EnemyShip prefab นี้อยู่แล้ว ไม่ต้องตั้งค่าเพิ่ม ให้ศัตรูเกิดใหม่หรือเริ่ม Play ใหม่เพื่อใช้ hitbox ใหม่ ตรวจค่า serialized, marker GUID และ reference manager→prefab แล้วผ่าน ไม่มีการแก้สคริปต์จึงไม่คอมไพล์ซ้ำ ยังไม่ได้ทดสอบยิง/ดูภาพใน Play Mode

อัปเดต 2026-10-27 เพิ่มคำอธิบายควบคุม: ShipHUD มี Controls Text รับ TMP_Text ของผู้ใช้ และ Controls Description ข้อความอังกฤษหลายบรรทัดแก้ใน Inspector ได้ ใส่ข้อความตอนเปิด HUD ไม่สร้าง/เปิดปิด/ย้าย UI ผู้ใช้ต้องลาก Text ลง Controls Text ก่อนเห็นคำอธิบาย รายละเอียดใน [UI/README.md](UI/README.md) คอมไพล์ผ่าน ยังไม่ได้ดูภาพ Play Mode

อัปเดต 2026-10-27 แก้ตายแล้ว Game Over ไม่ขึ้น: SampleScene ใช้ Game Over/Success Panel ร่วม object เดียว โค้ดเก่าตอนแพ้เปิดแล้วปิด object เดิมตาม success=false แก้ ExplorationRun.Finish ให้ไม่ปิดซ้ำเมื่อใช้แผงร่วม และอัปเดตหัวข้อ Game Over/Exploration Complete ตอนแสดงจริง ไม่แก้ UI/reference ไม่มีช่องใหม่ ไม่ต้องตั้งค่าเพิ่ม คอมไพล์ผ่าน ยังไม่ได้ตรวจภาพ Play Mode รายละเอียดใน [UI/README.md](UI/README.md)

## เพิ่มความไวหัน/เลี้ยวผู้เล่นเล็กน้อย (2026-10-27)

ตามผู้ใช้ขอให้หันยิงตามทัน ปรับ ShipController ค่าเริ่มต้นและ prefab ผู้เล่น `Examples/StarterShip/StarterShip.prefab`, `Examples/HeavyShip 2/StarterShip.prefab`: Turn Degrees Per Second 45 → 54, Pitch Degrees Per Second 30 → 36, Bank Degrees Per Second 60 → 75 และ Steering Response Time 0.4 → 0.32 วินาที ยังคง Max Pitch Angle ±30 องศา, Bank Angle 25 องศา และ Speed Response Time 0.6 เดิม ตัวคูณ Wing ยังคงมีผลตามเดิม ไม่มีช่องใหม่ ไม่เปลี่ยนความเร็วบิน/ศัตรู/การเล็งปืน/กล้อง/UI SampleScene สืบทอด prefab โดยไม่มี override ค่ากลุ่มนี้ ไม่ต้องตั้งค่าเพิ่ม เริ่ม Play ใหม่เพื่อโหลดค่า

ตรวจค่า serialized/การสืบทอดและคอมไพล์ runtime ด้วย Roslyn/references Unity จริงผ่าน ยังไม่ได้ลองบังคับบินหรือวัดความรู้สึกหันยิงใน Play Mode ตัวเลขนี้แทนเพดาน 30/45 และหน่วง 0.4 ในบันทึกเก่าก่อนหน้า

## วัสดุจากศัตรูและสีชุดใหม่ (2026-10-27)

แก้ `Enemy.cs` เพิ่มรับวัสดุทันทีตอน HP ลดจนตายจริงทุกตัว: สุ่ม 1 ชนิด จำนวน 1 ชิ้นจากวัสดุที่ valid ในพูลโรงซ่อมทั้งสี่ ตัด Item ID ซ้ำก่อนสุ่ม เพิ่มคลังผู้เล่นแล้วส่ง pickupCollected เพียงเมื่อ Add สำเร็จ จึงแสดงในแจ้งเตือน RepiarMat ระหว่างสู้และผลลัพธ์ตอนจบเดิม รวมถึงยอด Collected/สูตรโบนัส ไม่สร้างของลอยในโลกหรือ prefab ดรอปใหม่ตามผู้ใช้ยืนยัน ไม่จ่ายจากการปิด/ทำลายศัตรูเฉย ๆ และ IsAlive เดิมกันตาย/แจกซ้ำ

ช่องใหม่ **Enemy > Drop Material Pools** รับ RepairMaterialPool[] ผูก GunMaterials / ArmorPlateMaterials / WingMaterials / JetMaterials ใน `Examples/Enemies/EnemyShip.prefab` แล้ว EnemySpawnManager/SampleScene ใช้ prefab นี้ตามเดิม ไม่ต้องลาก reference เพิ่ม ตอนตรวจพูลทั้งสี่มีวัสดุ valid ชนิดเดียวกัน จึงได้ชนิดเดิมทุกครั้งจนเพิ่มชนิดในพูลหน้าซ่อม (ไม่เพิ่มวัสดุสมมุติ) หากไม่มีวัสดุ valid หรือคลังปลายทางจะ warning ไม่แจกปลอม; คลังเต็มจะไม่เพิ่ม/ไม่แจ้งว่าได้รับ

พบ Material ชุดใหม่ Model/Ship4, Ship5 และปืนบางชิ้นใช้ Universal Render Pipeline/Lit จึงถูก ShipRandomColors ที่รับเฉพาะ Toon/Toon ข้ามไป แก้ `ShipRandomColors.cs` ให้รองรับผิว Toon/Toon และ URP/Lit โดยใช้ palette เดิมหนึ่งชุดต่อลำทั้ง Enemy และ CustomerRepairShip ไม่เปลี่ยน shader/Material asset/texture/สีพื้นฐานในไฟล์หรือสียานผู้เล่น เพิ่มตรวจ HasProperty ก่อนอ่านสี และข้าม Renderer ที่อยู่ในรากไอพ่นปกติ/บูสต์และ VisualEffect ผ่าน ShipBoostEffect.ContainsEffect/ShipRandomColors.IsShipRenderer ใช้ตัวกรองเดียวใน RepairableShipPart ด้วย ไม่เปลี่ยน VFX/เส้นขอบหรือวิธีแฟลชคืนสีเดิม

ผลตรวจ: คอมไพล์ runtime ทั้งชุดด้วย Roslyn/references จริงของ Unity ผ่าน ตรวจ prefab ผูกพูล valid ครบ, สาย EnemySpawnManager → EnemyShip และ HUD/ผลลัพธ์ผูก Container อยู่แล้ว ตรวจ diff เฉพาะไฟล์งานนี้ผ่าน ยังไม่ได้ทดสอบฆ่าจริง/รับของ/เรนเดอร์สีใน Play Mode การตรวจ prefab+MaterialPropertyBlock ผ่าน Editor ที่ลองเตรียมไม่ได้ถูกรันในเซสชันนี้ จึงไม่อ้างว่าเห็นภาพแล้ว ลบเครื่องมือ/ผลคอมไพล์ชั่วคราว ผู้ใช้ไม่ต้องตั้งค่าเพิ่มสำหรับพูลและ UI ที่ผูกไว้แล้ว ต้องให้ศัตรูเกิดใหม่/เริ่ม Play ใหม่เพื่อใช้โค้ดที่แก้

อัปเดต 2026-10-27 ผลลัพธ์ตอนจบ: เพิ่มยอด Kills (ผู้เล่นฆ่าศัตรูในรอบนี้) และ Collected (จำนวนวัสดุที่เก็บรอบนี้ ไม่รวมโบนัส) ทั้ง Game Over/สำเร็จ แก้ Enemy.cs ส่งเหตุการณ์ตายจริงและ ExplorationRun รับนับเฉพาะฝ่ายผู้เล่นก่อนจบรอบ เพิ่มช่อง Game Over/Success Kills Text และ Collected Text บน MainUI > ExplorationRun ให้ผู้ใช้ลาก TMP_Text ของแต่ละแผง ไม่สร้าง UI แทน คอมไพล์ผ่าน ยังไม่ได้ตรวจ Play Mode รายละเอียดใน [UI/README.md](UI/README.md)

อัปเดต 2026-10-27 ผลลัพธ์ตอนจบ: แสดงวัสดุที่เก็บรอบนี้รวมหนึ่งแถวต่อชนิด จำนวนโบนัสต่อท้ายในแถวเดียว เช่น +20 +2 เปิดรูป Bonus Icon เฉพาะชนิดที่ได้รับโบนัสจริง เพิ่ม Game Over/Success Materials Container และ Result Row Prefab บน ExplorationRun (ผูก prefab วัสดุเดิมแล้ว) กับ Bonus Icon บน RepairMaterialDragItem ผู้ใช้ต้องสร้าง/ลาก Container และรูปโบนัสเอง ไม่แก้ layout หรือสร้าง UI Toolkit ดู [UI/README.md](UI/README.md) คอมไพล์ผ่าน ยังไม่ได้ตรวจภาพ Play Mode

## สำรวจ 30 วินาทีและผลลัพธ์ (2026-10-27)

เพิ่ม ExplorationRun บน MainUI ของ SampleScene: ยานตายขึ้น Game Over; ครบ 30 วินาทีโดยยังรอดขึ้น Exploration Complete หยุดฉากหลังจนกดกลับ Repair เก็บวัสดุไว้แม้แพ้ สำเร็จรับโบนัสสุ่มจากชนิดที่เก็บรอบนี้ รวม 10% ของจำนวนรวมปัดลง จ่ายครั้งเดียวผ่านคลัง/เซฟเดิม เพิ่ม Scripts/ExplorationRun.cs และ ExplorationRewards.cs ไม่สร้าง UI Toolkit หรือหน้าตา UI ผู้ใช้ต้องสร้างและผูก Game Over/Success Panel และ Back Button บน component ใหม่; Timer/Title/Bonus Text เป็น optional อ่านช่อง/วิธีเปิดปิด/ข้อจำกัดใน [UI/README.md](UI/README.md) คอมไพล์ผ่านและทดสอบโบนัส 19 checks ยังไม่ได้ทดสอบ Play Mode

อัปเดต 2026-10-27 หน้าปรับแต่ง: กดเลือกชิ้นส่วนแล้วเปลี่ยนโมเดลในช่องนั้นของ CustomizationPreview ทันที รวมของที่ยังล็อก ไม่ซื้อ/ใส่จริงจนกดปุ่มเดิม เปลี่ยนหมวดหรือปิดหน้าจะคืนส่วนที่ไม่ได้ทดลองเป็น Equipped จริง แก้ ShipCustomization.cs ไม่มีช่องใหม่ ไม่ต้องตั้งค่าเพิ่ม ดู [Customization/README.md](Customization/README.md) คอมไพล์ผ่าน ยังไม่ได้ตรวจภาพ Play Mode

## แจ้งเตือนของที่เก็บได้ (2026-10-27)

ShipHUD บน MainUI ใน SampleScene เพิ่ม Pickup Container / Pickup Row Prefab / Pickup Notification Duration ผูก prefab วัสดุหน้าซ่อม `UI/pre/RepiarMat.prefab` และเวลา 3 วินาทีแล้ว เสกหนึ่งแถวรูป/ชื่อ/+จำนวนเมื่อ ShipPickup เก็บสำเร็จ แล้วลบโดยไม่มีเฟด ไม่แจ้งตอนโหลดเซฟหรือใช้วัสดุ ไม่สร้าง/จัด Container ให้ ผู้ใช้ยังต้องสร้างและลาก RectTransform ลง **Pickup Container** รายละเอียดไฟล์/ช่อง/ผลตรวจอยู่ [UI/README.md](UI/README.md) ยังไม่ได้ตรวจภาพหรือ Play Mode เมื่อมี Container จริง

## ชิ้นส่วน 5 แบบต่อหมวดและคูลดาวน์หลบล่าสุด (2026-10-27)

อัปเดตพูล/ร้านล่าสุด: พูลต่อสู้บน EnemySpawnManager.prefab และพูลโรงซ่อมบน CustomerRepairManager ใน Repair.unity มี Gun/Plate/Wing/Jet หมวดละ 5 ครบ 20 ชิ้น แก้ reference Wing/Jet โรงซ่อมผิดหมวดด้วย เริ่มเกมปลดล็อกเฉพาะ Base ทั้งสี่ส่วน อีก 16 ชิ้นต้องซื้อราคา 800–1800 (Tank Wing 850) ตาม [Customization/README.md](Customization/README.md) ไม่ล้างของที่ซื้อในเซฟเก่า ไม่มีช่องใหม่และไม่ต้องตั้งค่าเพิ่ม ตรวจข้อมูลแล้ว ยังไม่ได้ทดสอบซื้อ/สุ่มใน Play Mode

เพิ่ม Plate/Wing/Jet ตามรายการผู้ใช้ เปลี่ยนชื่อ asset/prefab โดยคง GUID และ Part ID ของชุดเดิม ใช้ E สลับ Unity Plate/Wing พร้อมกัน เก็บของเป็นดูดเข้าตัวอัตโนมัติ และคูลดาวน์หลบ **1 วินาทีหลังจบท่า** แทน 4 วินาทีที่ระบุในบันทึกเก่า Twin Jet หลบสองครั้งต่อชุดแล้วคูลดาวน์หลังครั้งที่สอง Barrier อัตโนมัติใช้ VFX/Barrier เดิม ศัตรูใช้โล่/สเตตัส/Tank ก่อน ไม่เพิ่ม AI บูสต์ หลบ หรือสลับโหมด รายการไฟล์ ค่าทดลอง ช่อง Inspector ใหม่ และขอบเขตการตั้งค่าพูลอยู่ [Examples/README.md](Examples/README.md)

## Material ยาน Toon พร้อมเส้น (2026-10-26)

อัปเดต 2026-10-27: ยานลูกค้าที่สุ่มบินเข้าโรงซ่อมใช้ Outline World Width 0.03 บน CustomerRepairShip แยกจากเส้นยานทั่วไป 0.012 และกำหนดค่าให้ชิ้นส่วนโดยตรงตอนประกอบก่อนเริ่มบิน พร้อมรักษาค่าเส้นเมื่ออัปเดตสีพัง/ไฮไลต์ รายละเอียดและช่องปรับอยู่ `Repair/README.md` ไม่ต้องตั้งค่าเพิ่ม

ตามคำขอผู้ใช้ล่าสุด เปลี่ยนเฉพาะ Material ของชิ้นส่วนยานที่ใช้งานจริงเป็น `Toon/Toon` จาก Unity Toon Shader ที่ติดตั้งในโปรเจกต์: `Model/Ship1`, `Model/Ship2`, `Model/Ship3` หมวดละ Material.001/Material.002 รวม 6 ไฟล์ และ `Examples/StarterShip/Armor.mat`, `Metal.mat` ของปืน รวม 8 ไฟล์ แก้ไฟล์เดิม/เก็บ GUID สีหลัก texture และ UV เดิม ไม่สร้าง Material สำเนา ไม่แตะ Energy/Hull/ชุดประกอบตัวอย่างเก่าที่ผู้ใช้ยืนยันว่าไม่ได้ใช้ และไม่เปลี่ยน shader ของกระสุน/ไอพ่น/VFX Graph

StarterShip เดิมอ้าง Material ฝังใน Ship1.fbx: ตรวจ object ID และชื่อ/สีจากข้อมูลนำเข้าของ Unity แล้วผูก ArmorPlatePart/WingPart/JetPart และ renderer ที่อ้างข้อมูลเดียวกันใน `Scenes/Repair.unity` ไปยัง Material.001/Material.002 ที่มีอยู่ใน Model/Ship1 ไม่เปลี่ยน mesh ตำแหน่งหรือสเกลชิ้นส่วน ชุดผู้เล่น ศัตรู ยานลูกค้าซ่อม และหน้าคัสตอมใช้ prefab ชิ้นส่วนชุดเดียวกันจึงได้รับ Material ใหม่ด้วย

เปิดเส้นดำด้วย outline pass/keyword ของ Toon ตั้งสีเงาสองระดับจากสีหลัก (65% และ 40%) เพิ่ม `Scripts/ShipToonOutline.cs` บน root ของ Part prefab ทั้ง 12 ชิ้นใน StarterShip/HeavyShip 2/BarrierShip เพื่อชดเชยความหนาเส้นตาม lossyScale ผ่าน MaterialPropertyBlock ใน Edit/Play Mode ไม่สร้าง Material ระหว่างเล่นและไม่เขียนทับ property สีอื่น ช่องใหม่ **Outline World Width** บน **ShipToonOutline** คือความหนาเส้นเป้าหมายในหน่วยโลก ค่าเริ่มต้น **0.012**; 0 ซ่อนเส้นของชิ้นนั้นได้ ใส่ค่าและผูก component ให้แล้ว ไม่ต้องลาก reference เพิ่ม สเกลไม่เท่ากันแต่ละแกนใช้แกนใหญ่สุดเพื่อไม่ให้เส้นหนาเกิน โดยความหนาจริงยังขึ้นกับ normals/ระยะกล้อง/สูตร shader

ปรับ `ShipRandomColors`, `Enemy` (hit flash), `CustomizationPreviewPart` และ `RepairableShipPart` ให้ปรับสีเงาของ Toon ควบคู่สีหลัก การสุ่มสี/แฟลชขาว/ไฮไลต์/แสดงความเสียหายยังใช้ MPB และคืน property block เดิม ไม่มีช่อง Inspector เพิ่มในสคริปต์เหล่านี้

ผลตรวจ: คอมไพล์ Runtime ทั้งชุดรวม ShipToonOutline ด้วย Roslyn/references จริงของ Unity ผ่าน exit 0 ตรวจ Material ทั้ง 8 ไฟล์มี shader/keyword/ความหนาเส้นถูกต้อง ไม่มี property ซ้ำ ตรวจ component และ fileID ใน prefab 12 ไฟล์ไม่ซ้ำ ตรวจสูตรชดเชย 5 สเกล (0.005/0.16/0.45/1/100) ผ่าน และตรวจ diff ผ่าน ลบเครื่องมือชั่วคราวแล้ว ยังไม่ได้ให้ Unity คอมไพล์ shader/ตรวจภาพเส้นหรือทดสอบ Play Mode จริง ผู้ใช้ไม่ต้องตั้งค่าเพิ่ม หากกำลัง Play ให้ออกก่อนเพื่อให้ Unity โหลด asset และสคริปต์ใหม่

อัปเดตสี NPC 2026-10-27 ตามโทนที่ผู้ใช้เลือก: ยานศัตรูและยานลูกค้าสุ่มหนึ่ง palette ต่อหนึ่งลำจาก 8 ชุดสีหลัก/สีรอง/โลหะเข้มใน `ShipRandomColors` เช่น น้ำเงิน–ทองอุ่น, เขียวอมฟ้า–ครีม, แดงไวน์–ทราย, เขียว–ทอง, ม่วง–มิ้นต์, ส้มอิฐ–ครีม, งาช้าง–น้ำเงิน และเทาอมฟ้า–เหลืองทอง ไม่สุ่ม HSV แยกทุก material slot อีก ตัวถัง/ปีกใช้สีหลัก ปืน/Jet ในช่องสีสว่างใช้สีรอง ช่อง Material เดิมที่มี RGB สูงสุดน้อยกว่า 0.5 ใช้สีโลหะเข้มเหมือนกันทั้งลำ สุ่มครั้งเดียวตอนประกอบและคงสีจนยานถูกทำลายหรือประกอบใหม่

แก้ `ShipRandomColors.cs`, `Enemy.cs`, `CustomerRepairShip.cs`, `RepairableShipPart.cs` ใช้ palette เดียวผ่านชิ้นส่วนทั้งสี่และคำนวณสีเงาจากสีเดียวกัน คง alpha/texture/เส้นขอบ/แฟลชขาว/ความหม่นและไฮไลต์ด้วย MaterialPropertyBlock เดิม สุ่มเฉพาะผิว Toon/Toon ไม่เปลี่ยนสี VFX/ไอพ่น และไม่แก้ Material asset หรือสียานผู้เล่น ไม่มีช่อง Inspector ใหม่ ชุดสีปรับได้ใน array palettes ของ ShipRandomColors ไม่ต้องตั้งค่าเพิ่ม ยานที่ประกอบอยู่ก่อนแก้ต้องเกิดใหม่/เริ่ม Play ใหม่เพื่อใช้ระบบใหม่

ผลตรวจสี NPC: คอมไพล์ Runtime ทั้งชุดกับ Roslyn/references จริงของ Unity ผ่าน exit 0 และตรวจ diff ผ่าน ยังไม่ได้ตรวจภาพสีบนโมเดลหรือทดสอบ Play Mode จริง ลบไฟล์คอมไพล์ชั่วคราวแล้ว

เพิ่มระบบซ่อมลูกค้าและ ResourceManager แล้ว ดูขั้นตอนตั้งค่าที่ [Repair/README.md](Repair/README.md) ฉาก `Scenes/Repair.unity` ผูกจุดเดิมและ prefab แล้ว ต้องใส่พูลวัสดุและ Canvas ของผู้ใช้ก่อนเล่นครบวงจร ShipController.Inventory/ShipPickup ใช้คลังกลางเมื่อมี ResourceManager

ระบบปรับแต่งยาน/ปลดล็อกด้วยเงิน/เซฟ JSON อยู่ที่ [Customization/README.md](Customization/README.md) มี catalog 4 หมวดและ preview ใน Repair.unity แล้ว ต้องใส่ Canvas กับ prefab แถวของผู้ใช้และรายการ Locked ที่ต้องการขาย

อัปเดต 2026-10-26 — Unity 6000.3.8f1, Input System 1.18.0, บินแบบเครื่องบิน 3D เปลี่ยนระดับความสูงแกน Y ได้จริง ไม่ล็อกระนาบ

## พฤติกรรมที่ยืนยัน

- ยานบินไปข้างหน้าตลอดเวลาตามหัว (local +Z) ด้วย Speed แม้ไม่ได้กดปุ่ม ปล่อยปุ่มแล้วบินต่อในทิศปัจจุบัน
- S เชิดหัวไต่ระดับ (+Y), W กดหัวลดระดับ (-Y) เทียบกับท่าบินปกติ จำกัดมุม ±30 องศา กดค้างไม่ตีลังกา ความเร็วก้มเงยไม่เกิน 30 องศา/วินาที และค่อย ๆ รับ/คลายคำสั่ง
- A/D เอียงลำซ้าย/ขวาแล้วเปลี่ยนเส้นทางบินตามมุมเอียงจริง; ปล่อยค่อยคืนปีกตรง และยังบินต่อในทิศใหม่ ไม่หันไปแกนโลก -X/+X โดยตรง
- Handling กำหนดอัตราก้ม/เงยและอัตราเลี้ยว โดยจำกัดเพดานด้วย Pitch Degrees Per Second (30) และ Turn Degrees Per Second (45) ตามลำดับ หน่วยองศาต่อวินาที การเล็งปืนใช้เมาส์แยกจากทิศบิน
- การเปลี่ยนความเร็วรวมถึงบูสต์ใช้เวลาตอบสนอง 0.6 วินาที และอัตราเลี้ยวใช้เวลาตอบสนอง 0.4 วินาที ไม่เปลี่ยนทันที ปล่อยปุ่มก้มเงยแล้วค่อยหยุดหมุนและคงมุมนั้นเพื่อไต่/ลดระดับต่อ
- มุมเอียงเริ่มต้น 25 องศา หมุนลำยานทั้งชุดรวม collider ด้วย Rigidbody ไม่ใช่แค่เอียงกราฟิก
- กด Shift ซ้ายหรือขวาค้างเพื่อบูสต์ขณะเคลื่อนที่ เกจเริ่มต้น 100 ใช้ต่อวินาทีตาม Jet ปล่อยปุ่มแล้วฟื้นหลังเวลาหน่วง ถ้าเกจหมดต้องปล่อยก่อนใช้ครั้งใหม่
- กด Space หนึ่งครั้งขณะกด A/D เพื่อพุ่งซ้าย/ขวาแนวราบเท่านั้นพร้อมหมุนลำหลบ 360 องศา ระยะต้นแบบ 5 หน่วย พุ่งใน 0.6 วินาที แต่หมุนช้าลงเป็น 1.2 วินาที อมตะตลอดท่า และคูลดาวน์ 4 วินาทีหลังจบท่า กด Space เปล่า ๆ หรือ A+D พร้อมกันไม่ทำท่า ยานยังบินหน้าต่อ กล้องค่อยไล่ตามตำแหน่งโดยไม่หมุนตามลำ
- คลิกซ้ายค้างยิงจาก Muzzle ไปจุดใต้เมาส์ในฉาก 3D โดนวัตถุใช้จุดชน ไม่โดนใช้จุดไกลตามแนวเมาส์ ทั้งสองปากยิงเข้าจุดเดียว ไม่เปลี่ยนการควบคุมยาน
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
| Scripts/ShipHUD.cs | ผูก UI ที่ผู้ใช้จัดไว้ แสดงเลือด เกจบูสต์ ความเร็ว และรับรายการวัสดุที่มาร์กจากระบบอื่น |
| Scripts/ShipTrackedMaterialRow.cs | ผูกแถว UI ของวัสดุหนึ่งรายการ: รูป ชื่อ จำนวนที่มี/ต้องการ |
| Scripts/ShipMaterialData.cs | ScriptableObject วัสดุ: รหัส ชื่อ และ Sprite |
| Scripts/ShipBoostEffect.cs | เปิด/ปิดเอฟเฟกต์บูสต์ที่อยู่ในแต่ละ Part Prefab รองรับ VisualEffect/VFX Graph |
| Scripts/ShipDodgeState.cs | ควบคุมเวลาท่าหลบ ทิศหมุน i-frame และคูลดาวน์ ด้วยนาฬิกาที่รับจากสคริปต์กลาง |
| Scripts/ShipDodgeCameraLag.cs | เพิ่มความหน่วงตำแหน่งและการเล็งกล้องเฉพาะช่วงพุ่งหลบ แล้วค่อยคืนค่ากล้องปกติ |
| Scripts/Enemy.cs | ประกอบยานศัตรู รวมสเตตัส บินไล่ ชะลอในระยะยิง รับดาเมจ และตาย |
| Scripts/EnemySpawnManager.cs | สุ่มข้อมูลชิ้นส่วนจากพูล 4 ช่อง และเกิดศัตรูรอบผู้เล่นตามเวลา |
| Scripts/EnemyAttackTelegraph.cs | เส้นบอกแนวโจมตีก่อน/ระหว่างยิง ใช้ Material ชั่วคราวในหน่วยความจำ |
| Scripts/EnemyCombatMotion.cs | คำนวณความเร็วเข้าหาหรือถอยห่างเพื่อยิงรักษาระยะด้านหน้าผู้เล่น |
| Scripts/EnemyFireCycle.cs | ควบคุมยิงเป็นชุด 1 วินาที/พัก 1.5 วินาที และเตือนก่อนชุดถัดไป |
| Scripts/ShipDamageHitbox.cs | ทำเครื่องหมาย Trigger ที่ให้ปืนและกระสุนรับเป็นจุดโดนดาเมจได้ |
| Scripts/IShipHitReceiver.cs | รับดาเมจพร้อมฝ่ายผู้ยิง จุดชน และแนวผิว สำหรับ HIT/เลขดาเมจ |
| Scripts/ShipCombatFeedback.cs | กำหนด Prefab HIT / HIT Monster / DMG กลางในซีน |
| Scripts/DamagePopup.cs | เติมเลขใน DMG Prefab เดิม หันเข้ากล้อง เคลื่อน/เฟดและทำลายตามค่าที่มีใน Prefab |
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

ข้อมูลแต่ละ asset มี Part Prefab ของตัวเอง; ใน GunPart มี ShipGun ผูก Muzzle และ StarterBullet.prefab ไว้แล้ว ไม่ต้องลากช่องเพิ่มเพื่อประกอบตัวอย่าง ตรวจโครงสร้างไฟล์และ reference ของ instance แล้ว แต่ยังไม่ได้เปิดซีนตรวจภาพ/ทดสอบ Play Mode จริง SampleScene มี CinemachineCamera ตามยานตามรายละเอียดด้านล่าง

### กล้องตามยาน Cinemachine 3.1.7

อัปเดตระยะกล้องต่อสู้ 2026-10-26: เพิ่มระยะตามหลังยานอีก 2 หน่วยใน SampleScene ที่ CinemachineCamera > Cinemachine Follow > Follow Offset Z จาก -10 เป็น -12 (X=0, Y=3 เดิม) ระยะบูสต์จึงเพิ่มจาก -12 ไป -15 อัตโนมัติ และปล่อยคลิกขวาจะกลับมุมตามระยะฐานใหม่นี้ ไม่มีช่องใหม่ ไม่ต้องตั้งค่าเพิ่ม ตรวจค่า serialized และ diff แล้ว ยังไม่ได้ดูภาพใน Play Mode

อัปเดต 2026-10-26: หน้าต่อสู้กดคลิกขวาค้างแล้วลากเมาส์เพื่อหมุนกล้องรอบยาน ปล่อยแล้วค่อยคืนมุมตามหลังยานเดิม ใช้ ShipDodgeCameraLag ตัวเดิมหมุน Follow Offset ก่อน Cinemachine อัปเดต ยังเล็งยานผ่าน Rotation Composer และใช้ No Roll เดิม ระยะบูสต์/ความหน่วงตอนหลบทำร่วมกันได้ คืน offset เดิมเมื่อปิด component ไม่แก้ทิศบิน/การยิง/UI และไม่ล็อกหรือซ่อนเคอร์เซอร์

ช่องใหม่บน CinemachineCamera > ShipDodgeCameraLag กลุ่ม Right Mouse Camera Orbit: Orbit Sensitivity = 0.15 องศาต่อหน่วย delta เมาส์, Orbit Pitch Limit = 60 องศาสำหรับจำกัดก้มเงยจากมุมเดิม, Orbit Return Time = 0.35 วินาทีสำหรับความนุ่มตอนคืนมุม ใส่ค่าใน SampleScene แล้ว ไม่ต้องลาก reference หรือตั้งค่าเพิ่ม ไม่มีเมาส์/เกมเสียโฟกัส/ยานตายจะคืนมุมด้วย ใช้เฉพาะ component กล้องต่อสู้ ไม่แก้ RepairOrbitCamera

ผลตรวจ: คอมไพล์ Runtime ทั้งชุดด้วย Roslyn และ response file/references ของ Unity โปรเจกต์จริงผ่าน (exit 0) ตรวจ git diff --check ผ่าน ลบไฟล์คอมไพล์ชั่วคราวแล้ว ยังไม่ได้ลากกล้อง/ตรวจภาพหรือทดสอบร่วมกับบูสต์และหลบใน Play Mode จริง

ตั้งค่าใน `Scenes/SampleScene.unity` โดยใช้ CinemachineCamera และ CinemachineBrain ที่ผู้ใช้เพิ่มไว้แล้ว ไม่สร้างกล้องซ้ำและไม่เพิ่มสคริปต์กล้องเอง

- **Main Camera > Cinemachine Brain**: ใช้ Brain เดิม Smart Update และ Late Update สำหรับส่งผลกล้องจริง
- **CinemachineCamera > Tracking Target**: PlayerShip (ShipController) / Transform root ของยาน ผูกไว้แล้ว Look At ใช้เป้าหมายเดียวกันผ่าน Custom Look At Target = false
- **Position Control = Cinemachine Follow**: เพิ่ม component และตั้ง Follow Offset `(0, 3, -12)` คือหลังยาน 12 หน่วย สูง 3 หน่วย; Binding Mode = Lock To Target No Roll ตามก้มเงย/เลี้ยวของยานแต่ไม่เอียงภาพตามปีก
- **Follow > Position Damping**: `(0.15, 0.15, 0.15)` และ Rotation Damping `(0.1, 0.1, 0)` ใช้เป็นค่าต้นแบบ
- **Rotation Control = Cinemachine Rotation Composer**: เพิ่ม component เล็งกลางยาน วางยานกลางภาพ Damping `(0.1, 0.1)` ปิด Lookahead/Dead Zone/Hard Limits
- คง FOV เดิม 50 องศา และตั้งตำแหน่งเริ่มต้นของกล้องทั้งสองที่ `(0, 3, -10)` มองกลับไปหายาน กด Play ได้โดยไม่ต้องต่อช่องเพิ่ม
- ปรับระยะ/ความสูงที่ Follow Offset และความนุ่มที่ Damping บน CinemachineCamera ไม่แก้ Transform ของ Main Camera ระหว่างเล่น เพราะ Brain เป็นผู้ควบคุม
- ขณะ Shift บูสต์จริง **ShipDodgeCameraLag** จะค่อยเพิ่มระยะหลังยานอีก 3 หน่วย (Follow Offset Z จาก -12 ไป -15 ในค่าตัวอย่าง) และค่อยกลับระยะเดิมเมื่อปล่อย/เกจหมด ไม่เปลี่ยน FOV หรือความสูงกล้อง ทำร่วมกับ Dodge lag ได้
- ช่องใหม่กลุ่ม **Boost Camera Pullback** บน ShipDodgeCameraLag: **Boost Extra Distance** ระยะถอยเพิ่ม 3 หน่วย, **Boost Pullback Time** ความหน่วงตอนถอย 0.35 วินาที, **Boost Return Time** ความหน่วงตอนกลับ 0.6 วินาที ตั้งใน SampleScene แล้ว ไม่ต้องต่อ reference เพิ่ม สคริปต์จำ Follow Offset ตอนเริ่มเล่นและคืนเมื่อปิด component
- ตรวจ serialized field กับซอร์ส Cinemachine ใน package จริง ตรวจ Script GUID, target, component references และ fileID ของซีนแล้ว ยังไม่ได้ตรวจภาพ/ทดสอบ Play Mode จริง

### Prefab ประกอบสำเร็จ StarterShip (เพิ่มตามคำขอ 2026-10-26)

แก้การอ้างอิง Material ตามรายงานยานสีชมพู: Prefab เดิมเขียน external reference ของ `.mat` และ `.asset` เป็น `type: 3` ซึ่งเป็นชนิดอ้างอิงไฟล์นำเข้า แก้เป็น `type: 2` สำหรับ NativeFormat asset ครบ StarterShip, ชิ้นส่วนทั้ง 4 และ StarterBullet รวมถึงข้อมูลสเตตัส 4 ช่อง โดยคง GUID เดิมทั้งหมด ตรวจว่า Material มีจริง Shader GUID ตรงกับ URP Unlit ใน package และตรวจไม่เหลือ native reference ชนิดผิดแล้ว ยังไม่ได้ตรวจภาพใน Unity จริง ไม่เพิ่มช่อง Inspector และไม่ต้องลาก Material ใหม่ถ้าไม่ได้ override Renderer ใน instance เอง

- ใช้ `Assets/SpaceGarage/Examples/StarterShip/StarterShip.prefab` ลากลงซีนได้เลย ไม่ต้องกดเมนูสร้างชุดทดลองก่อน
- มีรูปทรงพื้นฐานสีฟ้า ปืนด้านหน้า เกราะกลาง ปีกซ้ายขวารวมเป็น Wing ส่วนเดียว และเครื่องยนต์คู่ด้านหลัง พร้อมวัสดุ URP Unlit 4 สี
- ผูกข้อมูล Gun.asset / ArmorPlate.asset / Wing.asset / Jet.asset และ Prefab ของแต่ละส่วนไว้ครบ พร้อม StarterBullet.prefab ทรงกลม ปืนยิงไป local +Z
- จุดเสกอยู่บนลูกยานชื่อ GunSpawnPoint `(0, 0.14, 0.85)`, ArmorPlateSpawnPoint `(0, 0, 0)`, WingSpawnPoint `(0, 0, -0.1)`, JetSpawnPoint `(0, 0, -1.05)` ทั้ง 4 จุดผูกกับช่อง Mount บน ShipController แล้ว ขยับได้ตามต้องการ
- เห็นชิ้นส่วนประกอบใน Edit Mode ได้เลย โดย ShipController เก็บ reference ชิ้นตัวอย่างในฟิลด์ซ่อน assembledParts เมื่อ Awake จะปิดและถอดชุดตัวอย่างก่อนประกอบจริงจากข้อมูล เพื่อไม่ให้เกิดส่วน/ปืนซ้ำ ไม่มีช่อง Inspector ใหม่ที่ผู้ใช้ต้องกรอก
- ลาก Prefab ลงซีนที่มีกล้องมองเห็นยาน แล้วกด Play เพื่อทดลองบิน 3D หากใช้ SampleScene มีกล้อง Cinemachine ตามยาน instance เดิมให้แล้ว ถ้าสร้างยานใหม่แทนตัวเดิมต้องเปลี่ยน Tracking Target ให้ตรงยานใหม่
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

### HUD ต่อสู้ตัวอย่างด้วย UI Toolkit (2026-10-26)

ผู้ใช้ยืนยันล่าสุดว่า **UI จริงใช้ Canvas** จึงใช้ **ShipHUD** บน **MainUI** ใน SampleScene ผูก ShipController ให้แล้ว เพิ่ม Auto Find Player ค้น Tag Player อัตโนมัติเมื่อไม่ใส่ Ship ผู้ใช้ยังต้องลาก TMP Text/Slider/Image ที่ออกแบบเองลงช่อง ไม่ได้สร้างหรือย้ายหน้าตา Canvas เดิม Toolkit ยังคงเป็นตัวอย่างปิดได้เท่านั้น

แก้ Toolkit ไม่ขึ้นโดยใช้ UIDocument.visualTreeAsset แทนการแทรก CloneTree เอง กำหนด root เต็มจอและ rebind เมื่อ root ถูกสร้างใหม่ ตรวจ Play Mode จริงพร้อมดูภาพ ScreenCapture แล้ว HUD ขึ้นครบและ root 1920×1080 รายละเอียดผลตรวจใน SpaceGarage/UI/README.md

ตามคำขอให้ทำหน้าตาตัวอย่าง เพิ่ม `CombatHUD_Example` ใน SampleScene แยกจาก UI เดิม แสดงเลือด บูสต์ ความเร็ว Dodge วัสดุมาร์กพร้อมรูป และเป้าเมาส์ ปิดที่ GameObject/component หรือช่อง Visible ได้ และแทน UXML จริงผ่าน Layout โดยไม่แตะระบบเกม รายละเอียดไฟล์ ช่อง Inspector สัญญาชื่อ element และข้อจำกัดอยู่ใน **SpaceGarage/UI/README.md**

ไม่ต้องต่อ reference เพิ่มในซีนตัวอย่าง กด Play เพื่อดู HUD ส่วนระบบมาร์กต้องส่งรายการให้ ShipCombatHUDToolkit.SetTrackedMaterials เมื่อมีข้อมูลจริง แยก ShipMaterialRequirement เป็นไฟล์กลางโดยไม่เปลี่ยน API ของ ShipHUD เดิม

### HIT, HIT Monster และเลข DMG (2026-10-26)

เพิ่ม GameObject **CombatFeedback** ใน `Scenes/SampleScene.unity` พร้อม **ShipCombatFeedback** และผูก Prefab ที่ผู้ใช้เตรียมไว้แล้ว:

| ช่องบน ShipCombatFeedback | Prefab / หน้าที่ |
| --- | --- |
| Hit Prefab | `VFX/HIT.prefab` เมื่อผู้เล่นยิงศัตรูและเลือดลดจริง |
| Hit Monster Prefab | `VFX/HIT Monster.prefab` เมื่อศัตรูยิงผู้เล่นและเลือดลดจริง |
| Damage Number Prefab | `VFX/DMG.prefab` ตัวเลขเมื่อผู้เล่นยิงศัตรูเท่านั้น |
| Hit Lifetime | 2 วินาที ก่อนทำลาย HIT ที่เสก ปรับตามความยาว VFX จริงได้ |

ไม่ต้องลาก reference เพิ่มในซีนตัวอย่าง เลือก CombatFeedback เพื่อเปลี่ยน Prefab ได้ ถ้าใช้ซีนอื่นต้องมี component นี้หนึ่งตัวและผูกทั้งสามช่องเอง ไม่สร้าง VFX หรือ UI สำรองเมื่อเว้นช่อง

- กระสุน ShipProjectile เก็บฝ่ายผู้ยิงจาก owner ตั้งแต่ Initialize จึงยังแยก HIT ได้แม้เจ้าของกระสุนถูกทำลายก่อนกระสุนถึงเป้า
- ใช้ตำแหน่งชนจริงและ normal จาก sphere sweep หรือ ClosestPoint กรณีเริ่มซ้อน collider เพื่อเสก HIT แยกจากตัวกระสุน ไม่ผูกเป็นลูกศัตรู จึงยังเห็นนัดที่ฆ่าศัตรูหลังยานถูกทำลาย
- ShipController และ Enemy รองรับ **IShipHitReceiver.TakeHit(amount, source, point, normal)** เลือก HIT ตามฝ่ายผู้ยิง/เป้าหมาย ตัวเลขคือเลือดที่หายจริงหลัง Def จำกัดไม่เกินเลือดก่อนโดน ไม่แสดงเลขดาเมจที่ผู้เล่นได้รับ
- ระหว่าง Dodge i-frame หรือดาเมจหลังเกราะเป็นศูนย์ จะไม่เสก HIT และไม่แสดงเลข ความเสียหายที่ไม่รู้ฝ่ายผู้ยิงหรือศัตรูยิงศัตรูไม่ใช้เอฟเฟกต์สองฝั่งนี้
- `TakeDamage(float)` เดิมยังใช้ได้กับระบบอื่น แต่ไม่มีฝ่ายผู้ยิง จึงไม่เรียก HIT/DMG กระสุน custom ที่ต้องการเอฟเฟกต์ชุดนี้ให้เก็บ `ShipCombatFeedback.SourceOf(owner)` ตั้งแต่เริ่ม แล้วเรียก TakeHit แทน TakeDamage พร้อมข้อมูลจุดชน (มี fallback TakeDamage สำหรับเป้าหมายที่ไม่รองรับ interface ได้)
- DMG Prefab เดิมมี component `DamagePopup` แต่สคริปต์หาย จึงเพิ่ม `DamagePopup.cs` และใช้ GUID เดิม `22c09ef2ba100a3499915918f3177915` ให้ reference เดิมกลับมาใช้ได้ ไม่เปลี่ยนฟอนต์ สี หรือขนาดที่จัดไว้ ไม่สร้าง Canvas ใหม่
- DamagePopup ใส่เลขลง TMP_Text อัตโนมัติ (มีช่อง Damage Text ถ้าจะลากเอง) หันเข้ากล้องจริง รักษา color/fontSize/scale ของ Prefab ใช้ค่า Launch Speed 4, Gravity 10, Side Speed 0.3–0.9, Lifetime 1, Fade Delay 0.7 และค่าป๊อปที่มีใน Prefab ผู้ใช้ปรับบน DMG ได้เอง Script ไม่เพิ่มระบบ critical หรือเปลี่ยนสีตามชนิดดาเมจในงานนี้
- ตรวจคอมไพล์ Runtime/Editor ทั้งหมดผ่าน ไม่มี error/warning และตรวจ 82 จุดของ Prefab/script/fileID ในซีนผ่าน ยังไม่ได้ดู HIT หรือเลขใน Play Mode จริง

### EnemySpawnManager และ Enemy (2026-10-26)

เพิ่ม `SpaceGarage/Examples/Enemies/EnemyShip.prefab` และ `EnemySpawnManager.prefab` พร้อมวาง Manager แบบ Prefab Instance ใน `Scenes/SampleScene.unity` แล้ว เปลี่ยน Tag ของ StarterShip.prefab root เป็น **Player** โดยไม่แก้ ProjectSettings (Player เป็น tag มาตรฐาน) ไม่เพิ่ม Enemy tag

**เลือก EnemySpawnManager ใน Hierarchy** เพื่อตั้งค่า:

| ช่อง | ข้อมูลที่รับ / ค่าในตัวอย่าง |
| --- | --- |
| Enemy Prefab | Prefab root ที่มี Enemy และ Rigidbody/collider ไม่ใช้ ShipController ของผู้เล่น ผูก EnemyShip ไว้แล้ว |
| Gun Pool | รายการ ShipPartStats แบบ Slot Gun |
| Armor Plate Pool | รายการ ShipPartStats แบบ Slot ArmorPlate |
| Wing Pool | รายการ ShipPartStats แบบ Slot Wing |
| Jet Pool | รายการ ShipPartStats แบบ Slot Jet |
| Minimum / Maximum Spawn Distance | ระยะรอบผู้เล่น 40–60 หน่วย |
| Spawn Interval | พยายามสร้างครั้งละหนึ่งตัวทุก 5 วินาที |
| Max Alive | ศัตรูจาก Manager นี้พร้อมกันสูงสุด 5 ตัว |
| Spawn Immediately | เปิดไว้ เกิดตัวแรกเมื่อเริ่มและผู้เล่นพร้อม |
| Spawn Clearance / Blocking Layers / Attempts | ตรวจจุดเกิดไม่ทับ collider ในรัศมี 3 หน่วย ไม่สน trigger ลองสุ่มได้ 12 ครั้ง |

พูลเป็นรายการตัวเลือกสำหรับสุ่ม ไม่ใช่ระบบ reuse GameObject ศัตรู แต่ละส่วนเลือกอิสระหนึ่งอันจากรายการที่ Slot ตรงและมี Part Prefab ค่า null/ผิด Slot/Prefab ที่มี Enemy หรือ ShipController ซ้อนจะไม่ถูกเลือก ถ้าพูลใดไม่มีตัวเลือกถูกต้อง Manager จะข้ามและแจ้งเตือน ไม่สร้างศัตรูไม่ครบส่วน ตัวอย่างมี asset เดิมอย่างละ 1 ตัว ให้เพิ่มหลาย asset ต่อพูลเองเพื่อให้ได้ชุดหลากหลาย

ตำแหน่งเกิดสุ่มบนทรงกลม 3D รอบตำแหน่งผู้เล่นปัจจุบัน รวมสูง/ต่ำ ไม่ใช่เฉพาะพื้น XZ ถ้าไม่มีผู้เล่น Tag Player หรือผู้เล่นตายจะไม่เกิดเพิ่ม ถ้าจุดเกิดถูกบังทั้งหมดจะข้ามรอบนั้น `SpawnOne()` เรียกจากระบบอื่นได้ และ `AliveCount` อ่านจำนวนศัตรูของ Manager นี้ได้ ไม่สร้าง UI

**EnemyShip / Enemy**:

- Root มี Enemy, Rigidbody, BoxCollider และลูก GunSpawnPoint / ArmorPlateSpawnPoint / WingSpawnPoint / JetSpawnPoint ผูกกับ Gun Mount / Armor Plate Mount / Wing Mount / Jet Mount ครบ Root ไม่มีโมเดลสำเร็จใน Edit Mode; ชิ้นส่วนถูกสร้างจริงตอนเล่นตามผลสุ่ม ใช้ Part Prefab ของผู้เล่นตรง ๆ รวมสเตตัส = Base Stats + ส่วนทั้ง 4 ไม่แก้ Material/กราฟิกของชิ้นส่วน
- ค่า Base Stats ตัวอย่าง: Attack 5 / Def 0 / Speed 10 / Handling 60 / Health 40 เพิ่ม Speed จาก 6 เป็น 10 ตามผู้ใช้ยืนยันเพื่อไล่ความเร็วพื้นฐานผู้เล่น 8 ได้ ค่าสเตตัสและการใช้ไอพ่นปกติอ่านจากส่วนเหมือนผู้เล่น แต่ไม่มีอินพุตผู้เล่น ไม่มีบูสต์ ไม่มี Dodge และไม่ใช้ค่าบูสต์ของ Jet
- อยู่ไกลบินไล่และหันเล็งผู้เล่น Tag Player โดยตรง การยิงไม่อิงการอยู่ในจอหรือหลังกล้อง เมื่ออยู่ใน **Fire Range = 30** และหัวเข้าเป้า **Fire Angle = 15°** ก็เริ่มเตือนและยิงตามปกติได้แม้กำลังย้ายเข้าจอ
- เมื่ออยู่ใกล้แต่หลังกล้อง/นอกจอ ใช้ **IsEnteringView** ขยับเข้าจุดในกรอบภาพพร้อมหันเล็งผู้เล่นไปด้วย แยกทิศเคลื่อนที่ออกจากทิศเล็ง จึงไม่ต้องหยุดยิงหรือรอให้เห็นในจอก่อน ไม่วาร์ป ไม่เพิ่มการบินวนหรือ Dodge
- ศัตรูทุกตัวเลือกบินถอยหลังยิงได้เมื่ออยู่ด้านหน้าทิศบินผู้เล่นและอยู่ในระยะยิง ไม่สุ่มแบ่งชนิด 50/50 ตัวเกิดด้านหน้าจากการสุ่มทรงกลมเดิมจะใช้พฤติกรรมนี้เองโดยไม่เปลี่ยน SpawnManager
- ช่องใหม่กลุ่ม **Front-facing Retreat Fire** บน Enemy: **Front Keep Distance = 20** หน่วย ระยะที่พยายามรักษา และ **Front Distance Response = 5** หน่วย ช่วงค่อยเปลี่ยนความเร็วเดินหน้า/ถอยหลัง หากไกลกว่าระยะจะเข้าหา หากใกล้กว่าจะบินถอยโดยหัวและปืนยังเล็งผู้เล่น ความเร็วไม่เกิน Speed ปกติ ไม่ใช่บูสต์ ระยะรักษาถูกจำกัดไม่ให้เกิน 80% ของ Fire Range
- ศัตรูด้านหลังยังบินไล่ยิง ความเร็วในระยะยิงค่อยลดตามระยะ โดย **Close Range Speed Multiplier = 0.25** เป็นค่าต่ำสุดใกล้ตัวผู้เล่น ไม่ลดเหลือ 25% ทันทีที่เข้า Fire Range จนไล่ไม่ทัน เปลี่ยนทิศและความเร็วอย่างนุ่มด้วย **Speed Response Time = 0.6**
- ช่องใหม่กลุ่ม **Enter The Player's View When Nearby** บน Enemy: **Player Camera** กล้องจริงของผู้เล่น (เว้นว่างใช้ Camera.main); **Screen Entry Range** ระยะเปิดโหมดเข้าจอ 45 หน่วย; **Screen Entry Forward Distance** จุดเข้าจออยู่เลยความลึกผู้เล่นไปด้านหน้าอีก 15 หน่วย; **Screen Edge Margin** ขอบกระตุ้น 0.08; **Screen Return Margin** ต้องเข้าภาพลึกถึงขอบ 0.2 ก่อนออกโหมด เพื่อไม่สลับโหมดไปมาที่ขอบภาพ
- ผูกค่าบน EnemyShip.prefab แล้ว ไม่ต้องลากกล้องให้ Prefab ศัตรู ซ้าย/ขวาที่จะเข้าภาพเลือกจากฝั่งที่ศัตรูอยู่ เป้าหมายเข้าจอขยับตามกล้องผู้เล่น และตรวจกรอบด้วย viewport ไม่ใช่ Renderer.isVisible ซึ่งอาจนับ Scene View ของ Editor เป็นการเห็น
- ตรวจคอมไพล์หลังเพิ่มโหมดเข้าจอผ่าน ไม่มี error/warning ยังไม่ได้ทดลองไล่เข้าจอหรือดูภาพใน Play Mode จริง
- อัปเดตยิงนอกจอ/บินถอยหลัง: ตรวจคอมไพล์ผ่าน และทดสอบ EnemyCombatMotion ตัวจริง 11 กรณี ครอบคลุมเข้าหา หยุดรักษาระยะ ถอยห่าง ความเร็วไม่เกินปกติ และเปลี่ยนต่อเนื่อง ยังไม่ได้ดูภาพการบิน/ยิงจริงใน Play Mode ตั้งค่าเพิ่มใน EnemyShip.prefab แล้ว ไม่ต้องต่อ reference เพิ่ม
- ยิงเมื่ออยู่ใน Fire Range และหัวหันเข้าเป้าใน **Fire Angle = 15°** มี **Attack Warning Time = 0.35** วินาทีก่อนชุดแรก จากนั้น **Fire Burst Duration = 1** วินาที สลับ **Fire Pause Duration = 1.5** วินาทีตามผู้ใช้ยืนยันล่าสุด (ไม่ใช่ค่าร่าง 4/3) ตั้งบน EnemyShip.prefab แล้ว ไม่เปลี่ยน Shots Per Second ของปืนผู้เล่น
- ระหว่างพักปืนศัตรูยังบินไล่/เข้าจอ/ถอยหลังตามปกติ เส้นเตือนชุดถัดไปแสดงช่วงท้ายของเวลาพัก จึงไม่บวก 0.35 เพิ่มทุกครั้งเมื่อเป้าหมายยังเข้าเงื่อนไขตลอด ถ้าโดนตีหรือหลุดเป้าระหว่างชุดจะหยุดชุดและพักก่อนยิงใหม่ ยิงนอกจอได้เหมือนเดิม ไม่มีการเล็งเมาส์หรือยิงล่วงหน้าดักทาง
- เพิ่มลูก **DamageHitbox** บน EnemyShip.prefab มี BoxCollider แบบ Is Trigger ขนาด **(4.2, 1.8, 4.5)** จุดกลาง `(0,0,0.2)` และ ShipDamageHitbox เพื่อให้เล็ง/ยิงโดนง่ายขึ้น Collider ชนปกติบน root ยังขนาดเดิม `(2.8,0.8,3.25)` ไม่ขยายการชนกำแพงหรือผู้เล่น
- ปรับขนาดรับกระสุนที่ **EnemyShip.prefab > DamageHitbox > BoxCollider > Size** หากใช้ Prefab ศัตรูอื่น ให้สร้างลูกพร้อม BoxCollider Is Trigger และ ShipDamageHitbox โดยวางใต้ root ที่มี Enemy ปืนและ ShipProjectile รับ Trigger ที่มี marker นี้เท่านั้น ยังข้าม Trigger ของเก็บ/โซนอื่น ดาเมจหนึ่งนัดถูกเรียกครั้งเดียวแม้มีทั้ง Hitbox และ collider หลัก
- ทดสอบ EnemyFireCycle ตัวจริงผ่าน 17 กรณี รวมยิงครบช่วง พักครบ 1.5 วินาที เตือนในเวลาพัก ขัดจังหวะและกลับมายิง ตรวจคอมไพล์ Runtime/Editor และ reference Hitbox ผ่าน ยังไม่ได้ทดสอบภาพการยิง/Hitbox ใน Play Mode จริง ไม่ต้องลากช่องเพิ่มในซีนตัวอย่าง
- ShipGun เพิ่ม API **SetAITarget(bool pressed, float attack, Transform owner, Transform target)** ส่วน SetTrigger เดิมของผู้เล่นยังยิงตามเมาส์
- EnemyAttackTelegraph ถูกเตรียมโดย Enemy ถ้ายังไม่มี ใช้ LineRenderer เส้นแดงจากปืนไปเป้าหมายก่อน/ระหว่างยิง ปิดเมื่อไม่ยิงหรือโดนตี ปรับ Width / Warning Color หรือใส่ Warning Line เองบน component นี้ได้ ถ้าเพิ่ม component ไว้ใน Prefab เองจะไม่สร้างใหม่ Material ของเส้นสร้างในหน่วยความจำและทำลายพร้อมศัตรู ไม่สร้างไฟล์ .mat
- **Hit Reaction Duration = 0.15** และ **Hit Knockback Speed = 3**: เมื่อโดนดาเมจที่ผ่าน Def จะหยุด AI/ปืนชั่วคราว ถอยจากผู้เล่นและแฟลชขาวด้วย MaterialPropertyBlock ก่อนคืนค่าของ Renderer เดิม ไม่เปลี่ยนไฟล์ Material
- ช่อง **Animator** เป็นทางเลือก ใช้ Trigger `Attack` เฉพาะเมื่อมี parameter นี้อยู่แล้ว ไม่สร้าง controller/คลิปแอนิเมชันใหม่
- ใช้ `IShipDamageable.TakeDamage` รับกระสุน สูตร damage = max(0, incoming - Def) เลือดหมดหยุดยิง/ไอพ่น เรียก Died แล้วปิดและทำลายยาน ไม่มีระบบดรอปวัสดุในงานนี้

ในซีนตัวอย่างไม่ต้องต่อ reference เพิ่มเพื่อเริ่มระบบนี้ ผู้ใช้ต้องเพิ่มตัวเลือกในพูลเพื่อให้มีหลายชุดจริง และหากใช้ยานผู้เล่นอื่นต้องตั้ง Tag Player ที่ root ของยานนั้นเอง ชิ้นส่วนปืนที่ใช้สคริปต์กระสุนแบบ custom ต้องรองรับ IShipProjectile หากต้องการรับค่าดาเมจ/เจ้าของจากปืน

ตรวจคอมไพล์ Runtime/Editor ทั้งหมดกับ Unity/Input System/TMP/UGUI/Cinemachine DLL ผ่าน ไม่มี error/warning ตรวจ 120 รายการของ reference ตัวอย่าง (สคริปต์ พูล Slot ชิ้นส่วน Prefab Tag และ fileID ในซีน) ผ่าน ยังไม่ได้ทดสอบการเกิด บิน ยิง รับดาเมจ หรือภาพจริงใน Play Mode

### HUD และวัสดุที่มาร์ก (2026-10-26)

เพิ่มสคริปต์อย่างเดียว ไม่สร้าง Canvas/Panel/Layout หรือจัดหน้าตา UI ใหม่ ผู้ใช้ต้องมี UI แล้วลากช่องดังนี้:

1. ติด **ShipHUD** บน GameObject UI ของผู้ใช้ ลาก PlayerShip ลง **Ship**
2. **Health Text / Boost Text / Speed Text** รับ TMP_Text แสดงเลือดปัจจุบัน/สูงสุด เกจปัจจุบัน/สูงสุด และความเร็วที่ผ่านการหน่วงแล้ว หน่วย Unity ต่อวินาที ช่องที่ไม่ใช้ปล่อยว่างได้
3. **Health Slider / Boost Slider** รับ Slider ของผู้ใช้ หรือใช้ **Health Fill / Boost Fill** รับ Image ที่ตั้ง Type = Filled ไว้เอง รองรับใส่ทั้งสองหรือเลือกอย่างใดอย่างหนึ่ง ไม่เปลี่ยนรูปแบบ/ช่วง min-max ของ Slider ไม่ยิง OnValueChanged ระหว่างอัปเดต
4. สร้างหน้าตาแถววัสดุหนึ่งแถวแล้วติด **ShipTrackedMaterialRow** ลาก **Icon** (Image), **Material Name** (TMP_Text), **Amount Text** (TMP_Text) ของแถวนั้นให้ครบ จากนั้นทำเป็น Prefab แล้วลากลง **Material Row Prefab** บน ShipHUD และลาก RectTransform พื้นที่รายการบน Canvas ลง **Material Container** ระบบจะเสกแถวจากรายการที่มาร์กเข้า Container ตามผู้ใช้ขอ
5. ผู้ใช้จัด LayoutGroup เช่น VerticalLayoutGroup/GridLayoutGroup และขนาดแถวเอง ระบบไม่เพิ่มหรือเปลี่ยน layout ให้อัตโนมัติ ถ้าไม่ใส่ Material Container ยังใช้ **Material Rows** แบบลากแถวสำเร็จทีละอันได้เหมือนเดิม

สร้างข้อมูลวัสดุด้วย **Create > Space Garage > Material Data**:

- **Item Id**: รหัสคลังที่ไม่ซ้ำ เช่น `Scrap` ต้องตรงกันทั้งระบบดรอป คลัง และมาร์ก (ตัวเล็ก/ใหญ่มีผล)
- **Display Name**: ชื่อที่แสดงให้ผู้เล่น ถ้าว่างใช้ Item Id
- **Icon**: Sprite รูปวัสดุที่ผู้ใช้เตรียมไว้
- ShipPickup เพิ่มช่อง **Material** ให้ลาก asset นี้ได้ ถ้าใส่จะใช้รหัสจาก asset; ถ้าไม่ใส่ยังใช้ Item Id เดิม จึงไม่กระทบของดรอปเก่า

HUD ไม่เลือกวัสดุเอง และไม่แสดงทั้งคลัง ระบบมาร์กภายนอกเรียก:

```csharp
hud.SetTrackedMaterials(new[]
{
    new ShipMaterialRequirement { material = scrapMaterialAsset, requiredAmount = 10 },
    new ShipMaterialRequirement { material = circuitMaterialAsset, requiredAmount = 5 }
});
```

แต่ละแถวแสดงรูป ชื่อ และ `จำนวนใน ShipInventory/requiredAmount` อัปเดตเมื่อเก็บของ ระบบเก็บข้อมูลมาร์กเป็นสำเนาของรายการที่รับ ไม่สร้างระบบมาร์ก/เซฟมาร์กในงานนี้ เรียก `SetTrackedMaterials(null)` เพื่อล้างรายการ; `SetShip(newShip)` เพื่อเปลี่ยนยานที่ HUD ติดตาม

เมื่อใส่ **Material Container** ระบบใช้โหมดสร้างแถวตามรายการมาร์ก ไม่ใช้ array Material Rows โดยต้องมี **Material Row Prefab** ที่เป็นแถวแยก ไม่ใช่ Container หรือ Canvas ทั้งอัน สร้างแถวเท่าที่ต้องใช้แล้วเก็บใช้ซ้ำ หากมาร์กลดลงจะ SetActive(false) เฉพาะแถวที่ระบบเสก เมื่อเพิ่มมาร์กจะนำกลับมาใช้ หากล้างรายการจะปิดทุกแถวที่ระบบเสก ไม่แตะลูกอื่นของ Container เช่นหัวข้อ/ของตกแต่ง เมื่อปิด ShipHUD หรือเปลี่ยน Container/Prefab จะลบเฉพาะ instance ที่ระบบเป็นเจ้าของและสร้างใหม่เมื่อใช้งาน

ในโหมด **Material Rows** เดิม (ไม่ใส่ Container) แถวที่ไม่ใช้ยังล้างข้อความและปิดเฉพาะ Image ของรูปที่ไม่มีข้อมูล ไม่ปิด GameObject แถวหรือเปลี่ยน alpha หากจำนวนรายการเกินช่องที่เตรียมจะเตือนเหมือนเดิม

ตรวจคอมไพล์ Runtime/Editor หลังเพิ่ม Container ผ่าน ไม่มี error/warning ยังไม่ได้ทดลองกับ Row Prefab/Container ของจริงใน Play Mode ผู้ใช้ต้องลากสองช่องใหม่และเตรียม layout ของตัวเอง ไม่มีการสร้างกราฟิกแถวสำรอง

### เอฟเฟกต์บูสต์ในแต่ละชิ้นส่วน (2026-10-26)

เพิ่มช่อง **Normal Effect Root** บน ShipBoostEffect สำหรับไอพ่นปกติ ให้ลาก GameObject ลูกที่มี VFX Graph ปกติลงช่องนี้ ส่วนช่อง Effect Root / Effect Prefab เดิมใช้สำหรับไอพ่นบูสต์ ไม่เปลี่ยน reference VFX ที่ผู้ใช้ใส่ไว้แล้ว

| สถานะ | Normal Effect Root | เอฟเฟกต์บูสต์ |
| --- | --- | --- |
| บินปกติ / ปล่อย Shift / เกจหมด | เปิด | ปิด |
| กำลังบูสต์จริง | ปิด | เปิด |
| ยานตาย / ปิด ShipController / ปิดชิ้นส่วน | ปิด | ปิด |

สลับด้วย VisualEffect.Play/Stop และ SetActive จริง ปิดชุดเดิมก่อนเปิดชุดใหม่ ไม่สร้างเฟด ทั้งสอง Root ต้องเป็น GameObject ลูกคนละอัน ห้ามใช้ตัวเดียวกันหรือซ้อนเป็นพ่อ–ลูกกัน ส่วนไอพ่นปกติยังไม่ถูกสร้างให้ ผู้ใช้ต้องใส่ VFX และลากช่อง Normal Effect Root เพิ่ม 1 ช่อง; ช่องเดิมไม่ต้องต่อใหม่ รองรับกรณีใส่เฉพาะไอพ่นปกติหรือเฉพาะบูสต์ด้วย สคริปต์กลางส่ง SetFlightState เพื่อแยกบินปกติจากยานตาย

ใส่ **ShipBoostEffect** บน root ของ Part Prefab ที่ต้องการให้มีเอฟเฟกต์ จะเป็น Jet หรือส่วนอื่นก็ได้ แต่ละ Prefab ใช้ VFX คนละแบบได้ ไม่เก็บ VFX ไว้ใน ShipPartStats และไม่ต้องลากเอฟเฟกต์แต่ละอันบนสคริปต์กลาง

- วิธีฝัง VFX Graph: เพิ่ม GameObject ลูกที่มี VisualEffect ตั้ง Graph Asset ที่ผู้ใช้เตรียม แล้วลาก GameObject ลูกนั้นลง **Effect Root** ของ ShipBoostEffect
- วิธีเสกจาก Prefab: เว้น Effect Root ว่าง ลาก Prefab เอฟเฟกต์ที่มี VisualEffect ลง **Effect Prefab** และใส่ Transform ลูกของส่วนยานลง **Effect Spawn Point** ถ้าไม่ใส่จุด ใช้ตำแหน่ง root ส่วนนั้น
- ถ้าใส่ทั้งสองแบบ ใช้ Effect Root เป็นหลัก ไม่เสกซ้ำ Effect Prefab ถูกเสกครั้งเดียวตอนประกอบชิ้นส่วน จากนั้นเปิด/ปิด object เดิม ไม่ Instantiate ทุกเฟรม
- Effect Root ต้องเป็นลูกแยก ห้ามใส่ root ของชิ้นส่วนหรือตัวยาน เพื่อไม่ให้ระบบปิดยาน/ปืนไปด้วย ปรับตำแหน่งและทิศพ่นใน Prefab เอฟเฟกต์เอง
- ตอนยังไม่บูสต์: Stop VisualEffect และ SetActive(false) ที่ Effect Root; ตอนบูสต์จริงและมีเกจ: SetActive(true) และ Play VisualEffect ภายใน; ปล่อย Shift/เกจหมด/ยานตาย/ปิดสคริปต์จะหยุดและปิด Effect Root ทันที ไม่เพิ่มเฟดหรือปล่อยหางเอฟเฟกต์แทนเอง ใช้ event มาตรฐาน OnPlay/OnStop ของ VisualEffect
- ShipController ค้น ShipBoostEffect ทุกส่วนหลังประกอบหรือเปลี่ยน Loadout ใหม่แล้วสั่งตาม IsBoosting โดยอัตโนมัติ ส่วนที่ไม่ใส่เอฟเฟกต์ไม่สร้างของสำรอง
- ติด component นี้ไว้ใน **Examples/StarterShip/JetPart.prefab** แล้ว พร้อมลูก **BoostEffectPoint** ที่ `(0,0,-0.45)` ผู้ใช้ยังต้องใส่ Effect Prefab ของตนเอง หรือฝัง Graph แล้วลาก Effect Root ไม่มีการสร้าง/แก้ Graph ให้ในงานนี้

ตรวจคอมไพล์กับ Unity/TextMeshPro/UGUI/VFX DLL จริงผ่าน ไม่มี error/warning ตรวจ reference ของ JetPart แล้ว ยังไม่ได้ทดสอบ HUD หรือภาพ VFX ใน Play Mode จริง เพราะยังไม่มี UI และ Graph ของผู้ใช้ให้ผูก

### Shift บูสต์ความเร็ว (อัปเดต 2026-10-26)

ShipController อ่าน Shift ซ้ายหรือขวาแบบกดค้าง และใช้ผลบูสต์กับความเร็วเคลื่อนที่จริง ไม่ใช่เปลี่ยนเฉพาะกล้อง ยานตัวอย่างผูก Jet.asset ไว้แล้ว: ความเร็วปกติ 8 หน่วย/วินาที บูสต์เป้าหมาย 16 หน่วย/วินาที (2 เท่า) ค่อยเร่ง/ผ่อนด้วย Speed Response Time 0.6 วินาทีตามการบินมีน้ำหนัก

เกจเต็ม 100 ใช้ 25 ต่อวินาที บูสต์ต่อเนื่องได้ประมาณ 4 วินาที ปล่อย Shift หน่วง 1 วินาทีแล้วฟื้น 15 ต่อวินาที เกจหมดต้องปล่อยปุ่มก่อนเริ่มรอบใหม่ ปรับค่าบูสต์ใน Jet.asset ได้ ไม่เพิ่มช่องสคริปต์ ไม่ต้องต่อ reference เพิ่ม ตรวจเส้นทาง Input → เกจ → speedTarget → SmoothDamp → Rigidbody.MovePosition และค่าที่ผูกบน Prefab แล้ว ยังไม่ได้ทดสอบกดปุ่มใน Play Mode จริง

### Space + A/D หมุนหลบและ i-frame (2026-10-26)

- เก็บบูสต์ Shift ควบคู่กับท่าหลบตามผู้ใช้ยืนยันล่าสุด HUD บูสต์และ ShipBoostEffect เดิมยังใช้บูสต์ ไม่เปลี่ยนเป็นเอฟเฟกต์ท่าหลบ
- บน **ShipController** มี **Dodge Duration** (เวลาหมุนทั้งท่า 1.2 วินาที) / **Dodge Cooldown** (4 วินาทีหลังจบท่า) / **Dodge Distance** (5 หน่วย) และเพิ่ม **Dodge Dash Duration** (เวลาพุ่งด้านข้าง 0.6 วินาที) ตั้งใน StarterShip.prefab แล้ว ไม่ต้องต่อจุดหมุนใหม่ ถ้าตั้งเวลาพุ่งเกินเวลาท่า ระบบจำกัดให้จบพร้อมท่า
- Space ใช้ wasPressedThisFrame ไม่กดค้างต่อท่าอัตโนมัติ ต้องมี A หรือ D เพียงทิศเดียว ไม่เข้าคิวท่าระหว่างติดคูลดาวน์
- ท่าหลบหมุนรอบ local +Z 360 องศา คง yaw/pitch และมุมเอียงปกติระหว่างท่า ยานบินต่อไปตามหัว พร้อมพุ่งซ้าย/ขวาแนวราบตาม A/D ระยะ 5 หน่วย ทิศพุ่งอิง yaw อย่างเดียว ไม่อิง pitch/roll จึงไม่มีส่วนพุ่งขึ้น/ลงจาก Dodge (การบินหน้าต่อยังอาจไต่/ลดระดับตามหัวเดิม) พุ่งเร็วช่วงแรกแล้วค่อยชะลอให้จบใน 0.6 วินาที ส่วนหมุนใช้ 1.2 วินาที ไม่วาร์ป ไม่คืนตำแหน่งหลังจบท่า สามารถใช้ Shift บูสต์ที่แยกกันพร้อมท่าได้
- ช่วงหมุนทั้งหมด **IsInvulnerable** เป็น true และ **TakeDamage** จะไม่ลดเลือด ไม่ได้ปิด collider จึงยังชนกำแพงตามฟิสิกส์ คูลดาวน์นับหลังท่าจบ ไม่มีอมตะต่อในคูลดาวน์ ยกเลิกท่าเมื่อปิด controller แล้วไม่ล้างคูลดาวน์
- กล้องใน SampleScene ใช้ **Lock To Target No Roll** อยู่แล้ว การหมุนรอบแกนหน้าไม่เปลี่ยน forward ที่กล้องติดตาม จึงไม่เพิ่มการหมุนตามลำให้กล้อง ไม่เปลี่ยน Tracking Target หรือ World Up ของ Brain
- เพิ่ม **ShipDodgeCameraLag** บน **CinemachineCamera** ใน SampleScene และผูก **Ship** กับ PlayerShip แล้ว ระหว่างหลบเพิ่ม **Dodge Position Damping** = 0.9 และ **Dodge Aim Damping** = 0.6 เพื่อให้ยานพุ่งนำและกล้องเล็ง/เคลื่อนตามทีหลัง ปรับเข้าสู่ค่าหลบด้วย **Lag Blend In Time** = 0.08 วินาที และค่อยคืนค่าปกติด้วย **Lag Recovery Time** = 0.8 วินาที ไม่เปลี่ยน offset หรือ binding mode
- สคริปต์เก็บค่า Damping ปกติเมื่อเริ่มเล่นและคืนค่าเดิมเมื่อปิด component ไม่แก้ค่าของกล้องอื่น ผู้ใช้ไม่ต้องลาก reference เพิ่มในซีนตัวอย่าง ถ้าย้ายไปซีนใหม่ให้ติด component นี้บน CinemachineCamera ที่มี CinemachineFollow/RotationComposer และลาก ShipController ลง Ship
- API: **TryDodge(-1)** ซ้าย / **TryDodge(1)** ขวา คืน false ถ้าใช้ไม่ได้; **IsRolling**, **IsInvulnerable**, **CanDodge**, **DodgeCooldownRemaining**, **DodgeReadyProgress** ใช้เชื่อมระบบอื่น
- บน **ShipHUD** เพิ่มช่องแยก **Dodge Cooldown Text** (TMP_Text), **Dodge Ready Slider** (Slider), **Dodge Ready Fill** (Image แบบ Filled) ให้ผู้ใช้ลาก UI ที่เตรียมเอง แสดงเวลาคงเหลือเป็นวินาที: ระหว่างหมุนแสดง 4.0, หลังจบท่านับ 4 → 0, พร้อมใช้แสดง 0.0; แถบเติมจากว่างไปเต็มตามความพร้อม เว้นว่างได้ ไม่สร้างหรือเปลี่ยนหน้าตา UI เอง
- ทดสอบ ShipDodgeState ตัวจริงผ่าน 17 กรณี: ทิศที่ใช้ได้/ไม่ได้ เวลาเริ่ม/จบอมตะ กันท่าซ้อน คูลดาวน์เริ่มหลังท่าจบ ใช้ซ้ำตรงขอบเวลา ยกเลิกแล้วไม่ค้างอมตะและไม่ล้างคูลดาวน์ ยังไม่ได้ดูภาพหมุน/กล้องหรือทดลองปุ่มจริงใน Play Mode
- หลังเพิ่มการพุ่งด้านข้าง ทดสอบตรรกะตัวจริงผ่าน 27 กรณี รวมพุ่งนำช่วงแรก ไม่ย้อนกลับ ไม่เกินระยะ และระยะรวม 5 หน่วยตรงกันที่ 15/30/50/120 Hz ตรวจคอมไพล์พร้อม Cinemachine DLL และตรวจ component/target ของกล้องในซีนแล้ว ยังไม่ได้ดูภาพจริงใน Play Mode
- อัปเดตหมุนช้าลง: ทดสอบเวลาพุ่ง 0.6 แยกจากเวลาหมุน 1.2 ผ่าน 25 กรณี รวมระยะคงที่หลายความถี่ ไม่พุ่งต่อหลังครบระยะ อมตะจนหมุนจบ และคูลดาวน์ 4 วินาทีหลังหมุนจบ ตรวจคอมไพล์ผ่าน ไม่มี error/warning ยังไม่ได้ทดสอบภาพจริงใน Play Mode

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
- **Bank Angle / Bank Degrees Per Second**: มุมเอียงสูงสุดและความเร็วเอียง/คืนตรง ค่าเริ่มต้น 25 องศาและ 60 องศาต่อวินาที ปรับได้บน ShipController
- ช่องใหม่กลุ่ม **Weighted Flight Response** บน ShipController: **Max Pitch Angle** มุมก้มเงยสูงสุด 30 องศา; **Pitch Degrees Per Second** เพดานความเร็วก้มเงย 30; **Turn Degrees Per Second** เพดานเลี้ยว 45; **Steering Response Time** ความหน่วงอัตราก้มเงย/เลี้ยว 0.4 วินาที; **Speed Response Time** ความหน่วงเพิ่ม/ลดความเร็วและบูสต์ 0.6 วินาที ค่าหน่วงมากยิ่งตอบสนองช้า ผูกค่าบน StarterShip.prefab แล้ว ไม่ต้องลาก reference เพิ่ม
- **Pickup Radius / Pickup Layers**: ระยะเก็บและเลเยอร์ของ collider ของดรอป
- **Health Changed / Boost Changed**: อีเวนต์ส่งค่าปัจจุบันกับค่าสูงสุดให้ UI ที่ผู้ใช้ทำในอนาคต; **Died**: อีเวนต์ตอนเลือดหมด

เมื่อ Awake ระบบประกอบ Prefab 4 ส่วนตามข้อมูลที่ใส่ไว้โดยอัตโนมัติ ส่วนที่ไม่ใส่ asset/Prefab จะไม่ถูกสร้าง และ asset ผิด Slot จะถูกข้ามพร้อมคำเตือน ไม่สร้างกราฟิกสำรอง

### ShipGun: ติดใน Prefab ส่วน Gun

#### ปืน 4 แบบ (2026-10-27)

แก้ตามผู้ใช้ยืนยันให้ควบคุม exposed float **Size และ Size 1 พร้อมกัน** ใน CataclysmBeam: Laser เปิด 10/10 ขณะกดค้าง ปล่อยเป็น 0/0; Rail เปิด 20/20 นาน 1 วินาทีแล้วเป็น 0/0 พักอีก 1 วินาที ShipBeamEmitter ตั้งทั้งสองเป็น 0 ก่อนเปิด VFX instance และคืน 0 ทั้งคู่เมื่อหยุด/ปิด/ทำลายปืน ตรวจว่ากราฟมีทั้งสอง property ก่อนใช้ ไม่แก้กราฟ VFX หรือเพิ่มช่อง Inspector ผู้ใช้ไม่ต้องตั้งค่าเพิ่ม ใช้ Laser Size/Rail Size เดิมเป็นค่าของทั้งสอง property

ผลตรวจ Size คู่: คอมไพล์ Runtime ทั้งชุดกับ Unity assemblies จริงผ่าน exit 0 และ diff ผ่าน ลบไฟล์คอมไพล์ชั่วคราวแล้ว ยังไม่ได้ตรวจภาพ VFX/ทดสอบ Play Mode จริง

อัปเดตตามผู้ใช้ยืนยัน: Laser และ Rail Gun ยิงตรงตามแกน +Z (forward) ของ Muzzle แต่ละจุดทั้ง VFX และ ray ดาเมจ ไม่เล็งเมาส์/เป้า AI ไม่ต้องผ่านการตรวจกล้องหรือ viewport ถ้าไม่มี Muzzle ใช้ forward ของ ShipGun ยานยังเลี้ยวตามระบบบินเดิม Laser กดค้างได้ไม่จำกัดเวลา ไม่มีรอบพัลส์หรือคูลดาวน์ ปล่อยจึง Size=0; Rail คงพัลส์ 1 วินาที/พัก 1 วินาทีเดิม แก้ ShipGun/ShipBeamEmitter ไม่มีช่อง Inspector ใหม่และไม่ต้องตั้งค่าเพิ่ม ปืน Projectile/Plasma/Getsuka ยังใช้การเล็งเดิม ช่อง Muzzle ต้องหันแกน Z ไปทิศยิงตามปกติ

ผลตรวจการยิงตรง: คอมไพล์ Runtime ทั้งชุดกับ Unity assemblies จริงผ่าน exit 0 ตรวจ diff ผ่าน ยังไม่ได้ทดสอบภาพทิศลำแสง/ดาเมจหรือกดค้างจริงใน Play Mode

เพิ่ม **Weapon Type** บน ShipGun: Projectile คือโหมดเดิม (ค่าเริ่มต้นของ prefab เดิม ไม่เปลี่ยนปืนที่ผู้ใช้จัดไว้), Laser, Rail Gun, Plasma และ Getsuka เลือกชนิดบน component ของ Gun Part prefab ที่ต้องการแล้วบันทึก prefab ไม่เพิ่มปืนชนิดใหม่ลง catalog แทนผู้ใช้ เพราะยังไม่ได้ระบุว่าชิ้นส่วนใดเป็นปืนชนิดใด ใช้ Muzzle 1/2, Attack และ Damage Multiplier เดิม การเล็งเมาส์/AI ใช้เฉพาะ Projectile/Plasma/Getsuka ส่วน Laser/Rail ยิงตรง ปากสองจุดยิงพร้อมกัน ปากเดียวกันไม่ยิงซ้ำ ค่าคูลดาวน์ด้านล่างแทน Shots Per Second เฉพาะโหมดใหม่

| Weapon Type | การทำงาน |
| --- | --- |
| Laser | กดค้างเปิด CataclysmBeam ด้วย Size=10 ปล่อยแล้ว Size=0 ไม่สร้างลำแสงใหม่ทุกเฟรม ทำดาเมจซ้ำทุก Laser Damage Interval (เริ่มต้น 0.1 วินาที) แต่ละครั้ง/ปากใช้ Attack × Damage Multiplier และผ่าน Def/Hit Feedback เดิม |
| Rail Gun | เริ่มยิงทำดาเมจครั้งเดียวต่อปาก เปิด Size=20 เป็นเวลา 1 วินาทีแล้ว Size=0 พักอีก 1 วินาที รวมรอบละ 2 วินาทีตามผู้ใช้ยืนยัน ปล่อยปุ่มระหว่างพัลส์ยังแสดงจนจบ 1 วินาที ไม่มีดาเมจซ้ำระหว่างเปิดลำแสง ยานตาย/ปิดปืนยกเลิกทันที |
| Plasma | ยิง Plasma Projectile Prefab ทุก 2 วินาทีขณะกดค้าง ยังเว้น prefab ให้ผู้ใช้ใส่กระสุนลูกกลมขนาดใหญ่ตามที่ขอ ระบบไม่สร้างกราฟิกทดแทน ขนาดภาพใช้ scale/โมเดลของ prefab ที่ผู้ใช้ใส่ |
| Getsuka | ยิง Getsuka Projectile Prefab ทุก 3 วินาที ใช้ VFX/Getsuka/Getsuka.prefab เดิม จัดแนว +Z ไปจุดเล็งและใช้ world up เพื่อให้เสี้ยวจันทร์ไม่เอียงตาม roll ของปากกระบอก ไม่เปลี่ยนกราฟ/mesh ของเอฟเฟกต์ |

ช่องใหม่กลุ่ม **Laser / Rail Gun**: **Cataclysm Beam** รับ VisualEffectAsset (.vfx) ที่มี exposed float ชื่อ Size; เลือก Laser/Rail ใน Inspector แล้ว OnValidate ผูก `VFX/JET/BulletBeam/CataclysmBeam.vfx` ให้อัตโนมัติถ้าช่องว่าง ไม่แทน reference ที่มีอยู่ **Laser Size**=10, **Rail Size**=20; **Laser Damage Interval**=0.1 วินาที; **Laser Range**=10 และ **Rail Range**=20 หน่วย คือระยะ ray ดาเมจจากแต่ละปาก ปรับได้แยกจาก Size ของภาพ VFX ต้องตรวจภาพในเกมเพื่อเทียบระยะจริงของกราฟ ลำแสงหยุดตรวจดาเมจที่ collider แรก ไม่ทะลุสิ่งกีดขวาง ข้ามเจ้าของ/กระสุนและรับ Trigger เฉพาะ ShipDamageHitbox ตามระบบเดิม

ช่องใหม่กลุ่ม **Plasma / Getsuka Projectiles**: **Plasma Projectile Prefab** รับ GameObject prefab ของลูกกลม (ต้องใส่เอง), **Getsuka Projectile Prefab** รับ prefab คลื่นพลัง (OnValidate ผูกของเดิมให้อัตโนมัติเมื่อเลือกโหมดและช่องว่าง), **Heavy Projectile Hit Radius**=0.8 คือรัศมีชนแบบ sphere sweep สำหรับ prefab ที่ไม่มีสคริปต์ IShipProjectile ถ้ายังไม่มีสคริปต์นี้จะเพิ่ม ShipProjectile เฉพาะ instance ที่ยิง แล้วส่ง Attack/ความเร็ว/อายุเดิมให้ ถ้ามีสคริปต์ IShipProjectile อยู่แล้วจะใช้ของผู้ใช้โดยไม่เพิ่มซ้ำ ความเร็วและอายุใช้ Projectile Speed/Projectile Lifetime เดิม รวมถึงขนาด hitbox ของ Getsuka เป็น sphere ไม่ใช่รูป mesh เสี้ยวจันทร์ ต้องปรับรัศมีให้เหมาะกับ prefab จริง

ไฟล์ที่เพิ่ม: `Scripts/ShipWeaponCycle.cs` (นาฬิกาพัลส์/คูลดาวน์และ enum ชนิดปืน), `Scripts/ShipBeamEmitter.cs` (VFX ต่อปาก สั่ง Size และทำลาย instance เมื่อปิดปืน) แก้ `ShipGun.cs` และเพิ่ม SetHitRadius ใน `ShipProjectile.cs` ไม่มีการแก้ prefab/scene/Gun Part ID หรือ reference ปาก/กระสุนเดิม ไม่มี UI ใหม่ การเปิด/ปิดลำแสงใช้ Size ตามผู้ใช้สั่ง ไม่ใช้การปิด root ยาน/ชิ้นส่วน

ผลตรวจ: คอมไพล์ Runtime ทั้งชุดกับ Unity assemblies จริงผ่าน ทดสอบ ShipWeaponCycle ตัวจริงด้วย Mono ผ่าน 16 กรณี: ขอบเขตพัลส์ Rail 1 วินาที/รอบ 2 วินาที, ยกเลิกพัลส์ไม่ข้ามคูลดาวน์, Plasma 2 วินาที, Getsuka 3 วินาที ตรวจ diff ผ่าน ลบเครื่องมือทดสอบชั่วคราวแล้ว ยังไม่ได้ทดสอบภาพ VFX/แนวคลื่น/การโดนดาเมจ/เส้นเตือน AI ร่วมปืนใหม่ใน Play Mode ผู้ใช้ต้องเลือก Weapon Type บนปืนแต่ละ prefab บันทึก prefab และใส่ Plasma Projectile Prefab เมื่อพร้อม ไม่มีการเลือกชนิดแทนจากชื่อชิ้นส่วน

- **Projectile Prefab**: เปลี่ยนเป็น GameObject Prefab ใส่กระสุนที่มีสคริปต์ชนิดใดก็ได้ ไม่บังคับ ShipProjectile โดยช่อง serialized ภายในใหม่ชื่อ projectilePrefabObject ส่วน reference เก่าเก็บเป็นช่องซ่อนเพื่อย้ายค่า/ใช้ย้อนหลัง ไม่ล้างกระสุนที่ผู้ใช้ใส่ไว้
- **Muzzle 1 / Muzzle 2**: Transform ตำแหน่งยิง กระสุนหัน +Z ของตัวเองเข้าจุดใต้เมาส์ ยิงพร้อมกันทั้งสองจุดตามผู้ใช้ยืนยัน ใส่เพียงจุดเดียวก็ยิงจุดเดียว ถ้าไม่ใส่ทั้งสองใช้ Transform ของ ShipGun ถ้าลาก Transform เดียวกันสองช่องจะไม่ยิงซ้ำ ใส่ Muzzle เดิมเป็น Muzzle 1 ให้อยู่แล้ว ผู้ใช้ต้องสร้างและลากจุดที่สองลง Muzzle 2 เองตามตำแหน่งโมเดล
- ช่องใหม่กลุ่ม **Mouse Aim** บน ShipGun: **Aim Camera** รับ Camera ที่เรนเดอร์จริง ไม่ใช่ CinemachineCamera ปล่อยว่างใช้ Camera.main (SampleScene มี MainCamera tag แล้ว ไม่ต้องลากเพิ่ม); **Aim Layers** เลเยอร์ Collider ที่รับเล็ง; **Aim Distance** ระยะ ray/ระยะเป้าหมายในอวกาศว่าง ค่าเริ่มต้น 1000 หน่วย
- หาเป้าหมายจากเมาส์หลังกล้องอัปเดตใน LateUpdate เลือกจุดชนใกล้ที่สุดที่ไม่ใช่ยานเจ้าของหรือ IShipProjectile รองรับ collider ปกติและ Trigger ที่มี ShipDamageHitbox เท่านั้น กระสุนสองปากใช้เป้าหมายเดียวกัน แต่ยังชนสิ่งกีดขวางตามระบบกระสุนของตนเองตามปกติ ไม่หมุนโมเดลปืนหรือยานตามเมาส์ ถ้าไม่มีกล้องหรือเมาส์อยู่นอก viewport กล้องจะไม่ยิง
- **Shots Per Second / Damage Multiplier / Projectile Speed / Projectile Lifetime**: ความถี่ยิง ตัวคูณ Attack ความเร็ว และอายุกระสุน
- Shots Per Second เป็นจำนวนจังหวะยิงต่อวินาที ไม่ใช่จำนวนกระสุนรวม เช่น 8 กับ 2 จุดยิงจะสร้าง 16 นัดต่อวินาที ดาเมจต่อกระสุน = Attack รวมของยาน × Damage Multiplier ไม่มีการใช้ Attack จากชิ้นปืนซ้ำสองรอบ
- Projectile มี Hit Radius และ Hit Layers เป้าหมายใช้ collider ปกติหรือ Trigger ที่มี ShipDamageHitbox และ implement IShipDamageable ถ้าต้องการรับดาเมจ กำแพงที่ไม่รับดาเมจก็หยุดกระสุนได้

#### กระสุนสคริปต์ชนิดอื่น

Prefab ใด ๆ ถูก Instantiate ตามจุดยิงได้เสมอ หากต้องการรับค่าจากปืน ให้สคริปต์กระสุน implement **IShipProjectile** และเมธอด `Initialize(Transform owner, float damage, float speed, float lifetime)` ปืนจะเรียก receiver ทั้งบน root และลูกหลังสร้าง Prefab กระสุนเดิม ShipProjectile รองรับ interface นี้แล้ว

กระสุนที่ไม่ implement interface ยังเสกและใช้ Awake/Start/ค่าของตัวเองได้ แต่จะไม่รับค่าดาเมจ ความเร็ว อายุ หรือเจ้าของจากปืนอัตโนมัติ สคริปต์กระสุนแต่ละชนิดรับผิดชอบวิธีเคลื่อนที่ ชน อายุ การทำลายตัวเอง และไม่โดนเจ้าของเอง ปืนไม่บังคับเพิ่ม ShipProjectile ลง Prefab ที่มีสคริปต์อื่น

อัปเดตเมนูสร้างชุดทดลองให้ใส่ GameObject ลงช่องใหม่แล้ว ตรวจคอมไพล์สคริปต์ทั้งหมดกับ DLL จริงผ่าน ไม่มี error/warning ยังไม่ได้ทดสอบยิงสองปากกระบอกหรือกระสุนชนิดใหม่ใน Play Mode จริง

### ShipPickup / ShipInventory

- ของดรอปใช้ ShipPickup กับ SphereCollider ตั้ง Is Trigger ไว้ กำหนด Item ID เช่น Scrap และ Amount
- วาง collider ให้ศูนย์กลางตรงวัตถุ ของเก็บต้องอยู่ใน Pickup Layers ของยาน
- ShipInventory.Items เก็บรายการในหน่วยความจำ; Count(itemId) อ่านจำนวน และ Add(itemId, amount) เพิ่มจำนวน; Changed แจ้งให้ UI อัปเดต

## เชื่อมหน้าปรับแต่งภายหลัง

เรียก `controller.SetLoadout(gun, armorPlate, wing, jet)` ด้วย ScriptableObject ทั้ง 4 ตัว รับ null ได้ ถ้าประเภทไม่ตรงช่องจะคืน false และคงชุดเดิม

ระหว่างเล่นจะปิด/ถอดชิ้นเก่า ประกอบ Prefab ใหม่ ค้น ShipGun ใหม่ และรวมสเตตัสใหม่ โดยไม่เติมเลือด/เกจฟรี (คงค่าปัจจุบันและจำกัดไม่ให้เกินค่าสูงสุดใหม่) หากตั้งก่อน Awake ระบบจะประกอบตอนเริ่มเล่น

`RebuildStats()` รวมค่าสเตตัสใหม่อย่างเดียว ไม่เปลี่ยน Prefab; `RebuildStats(true)` เติมเลือด/เกจเต็มสำหรับการเริ่มรอบใหม่ที่ระบบเกมอนุญาต

## ข้อจำกัดและผลตรวจ

- แก้รายงานบินวนกลับหัว: เดิมสะสม quaternion ก้มเงยได้ครบวง ทำให้ยานผ่านแนวดิ่งและกล้อง LockToTargetNoRoll เปลี่ยนกรอบอ้างอิงใหม่ เปลี่ยนเป็น yaw/pitch ที่จำกัดมุมก้มเงย ±30 องศาและ bank ±25 องศา จึงไม่ผ่านแนวดิ่ง ลดความเร็วเลี้ยวและเพิ่มความหน่วงตามผู้ใช้ขอ คง Cinemachine เดิมไว้
- การเปิดดู Unity ผ่าน computer-use ทำไม่ได้ในรอบนี้: native pipe เชื่อมต่อไม่สำเร็จ แม้ลองใหม่และ reset แล้ว จึงยังไม่อ้างว่าตรวจภาพหรือทดสอบ Play Mode จริง

- แก้การควบคุมตามผู้ใช้ยืนยันล่าสุดเป็นเครื่องบิน 3D: S เชิด / W กดหัว / A,D เอียงแล้วเลี้ยว บินต่อเนื่อง ปลด Constraints ของ Rigidbody และลบ Bank Visual Root ตัวอย่างเดิมไม่ต้องตั้งค่าเพิ่ม ค่ามุมเอียง 0 ทำให้ไม่เลี้ยวด้วย A/D เพราะไม่มีการเอียง

- ตรวจคอมไพล์สคริปต์ Runtime และ Editor กับ DLL จริงของ Unity 6000.3.8f1 และ Input System ด้วย Roslyn ผ่าน ไม่มี error/warning
- ยังไม่ได้ทดสอบ Play Mode หรือดูภาพจริง จึงยังไม่ยืนยันความรู้สึกควบคุม/การชน/ภาพจากกล้อง
- สูตร Def ต้นแบบ: ความเสียหายที่ได้รับ = max(0, ดาเมจเข้า - Defense); เลือดหมดหยุดคุมและยิง เรียก Died ไม่ลบยานหรือสร้างหน้าแพ้เอง
- มีระบบศัตรูสุ่มส่วน/บินไล่ยิงแล้ว แต่ยังไม่มีสุ่มดรอปวัสดุ ส่วนปืนมี Projectile เดิมและโหมด Laser/Rail Gun/Plasma/Getsuka ตามหัวข้อ ShipGun แล้ว ยังต้องตรวจภาพและดาเมจจริงใน Play Mode
- ค่าฟื้นเกจ ตัวคูณบูสต์ สูตร Def และระยะเก็บเป็นค่าต้นแบบที่ปรับต่อได้ ไม่ใช่ข้อสรุปบาลานซ์เกม
- ไม่มีการแก้ SampleScene, UI, Input Actions เดิม หรือ Project Settings เมนูสร้างชุดทดลองต้องกดใน Unity เอง

## เสียงส่วนกลางและ Tutorial (2026-10-27)
ใช้ Scirpt/Tutorial/UISfxManager.cs เดิมเป็นศูนย์เสียง UI/BGM/ปืน/ไอพ่น ตัด dependency ระบบเก่าออก เชื่อม ShipGun, ShipController, Enemy, CustomerRepairShip และ CustomizationUI แล้ว ไม่มีการสร้างหรือปรับ UI ไม่มีช่องใหม่บนสคริปต์ยาน ช่องใหม่ทั้งหมดอยู่บน UISfxManager เป็น AudioClip ปืนแต่ละชนิด/Laser Loop/Normal Jet Loop/Boost Jet Loop พร้อม volume และระยะเสียง ผู้ใช้ต้องเพิ่ม manager ในซีนเริ่มและใส่คลิปเอง เปิด Dont Destroy On Load รายละเอียดและผลตรวจใน Scirpt/Tutorial/README.md คอมไพล์ผ่าน ยังไม่ทดสอบฟังเสียงจริง


## Start / Settings (2026-10-27)

เพิ่ม Scripts/StartMenuUI.cs, SettingsUI.cs, GarageSettings.cs สำหรับ Canvas/UGUI ของผู้ใช้ เปิดปิด SetActive ไม่มี layout/UI สำรอง Start เรียก ResourceManager.BeginGame เฉพาะครั้งแรกที่ไม่มีเซฟ แล้วเปิด Main UI ครั้งต่อไปใช้เซฟเดิม `newGame` ไม่เริ่ม/reset อัตโนมัติหรือต้อง reload ซีนแล้ว RepairUI เพิ่ม SetMainMenuVisible กันเปิดทับ Start ตั้งเสียงรวม/เพลง/เอฟเฟกต์เชื่อม UISfxManager และเลือกความละเอียด+เต็มจอ/หน้าต่างผ่าน Apply บันทึก PlayerPrefs ขนาดจอมีผลใน Build ไม่แตะ Game View

ผู้ใช้ต้องสร้าง UI และใส่/ลากช่อง StartMenuUI และ SettingsUI เอง รายการช่องและตำแหน่ง host อยู่ UI/README.md รายละเอียดเซฟใน Customization/README.md คอมไพล์ทั้งชุดผ่าน exit 0 ยังไม่ทดสอบกด UI/เสียง/Build และไม่แตะเซฟจริง

## วัสดุ 11 ชนิดและมอนดรอป 1–10 (2026-10-27)

สร้าง Repair/Materials/*.asset ใช้รูป UI/Item ครบ Screw + วัสดุสิบชนิดตามภาพ ผูกพูลซ่อมสี่ slot และ ResourceManager.prefab catalog/เริ่มต้นอย่างละ 10 แล้ว ลบ override รายการเก่าใน Repair.unity ให้สืบทอด prefab เซฟเดิมไม่เปลี่ยนจำนวน RepairMaterialOption เพิ่ม Allowed Parts กรอง Shield Crystal ให้เฉพาะ Barrier Plate; Plasma Core ใช้กับ Gun/Jet ทุกชิ้นตามผู้ใช้เลือก Enemy ดรอปหนึ่งชนิด จำนวนสุ่ม 1–10 เข้าคลังและ UI เดิม ไม่มีช่องใหม่บน Enemy หรือ UI ไม่ต้องลากเพิ่ม รายการทั้งหมดและผลตรวจใน Repair/README.md คอมไพล์และตรวจ reference ผ่าน ยังไม่ได้ทดสอบ Play Mode

## แก้ Build CS0006 จาก DLL ตรวจงานที่ลบแล้ว (2026-10-27)

Editor.log ล่าสุดมี CS0006 Assets/FlightCheck.dll และ Assets/PickupCheck.dll ไม่พบไฟล์เหล่านี้หรือ explicit reference ใน source/asmdef/rsp ของ Assets/ProjectSettings/Packages แต่แคช Library/Bee ของ Player ยังอ้าง plugin เก่า จึงให้ Unity รัน AssetDatabase.Refresh(ForceSynchronousImport|ForceUpdate) และ CompilationPipeline.RequestScriptCompilation(CleanBuildCache) ผ่านสคริปต์ Editor ชั่วคราว หลังผู้ใช้ Refresh สคริปต์ลบตัวเองพร้อม .meta แล้ว ไม่เพิ่ม DLL ปลอมหรือแก้ runtime/UI/เซฟ

ผลจริงจาก Unity Editor.log หลังซ่อม: GarageCacheRepair รายงาน refresh/ลบตัวเอง/request clean, Tundra build success (55.31 seconds), 826 items updated, 979 evaluated และ Mono successfully reloaded assembly ตรวจแคช Bee ใหม่ไม่พบ reference สอง DLL ใน rsp/inputdata/dag แล้ว ไม่มีไฟล์ตรวจชั่วคราวค้าง ยังไม่ได้สั่ง Player Build ซ้ำ ผลยืนยันนี้เป็นการคอมไพล์ Editor จริง ผู้ใช้ลอง Build อีกครั้งได้ ไม่มีช่องหรือ reference ใหม่ที่ต้องตั้งค่า

### CS0006 แก้ที่ฐานข้อมูลนำเข้าจริง (2026-10-27, แทนข้อสรุปรอบก่อน)

Build ซ้ำยังมี CS0006 แม้ Editor คอมไพล์ผ่าน จึงตรวจ PluginImporter ผ่าน Unity จริง พบ Assets/FlightCheck.dll และ Assets/PickupCheck.dll มี importer/GUID ค้างทั้งที่ File.Exists=false และ AssetDatabase.DeleteAsset คืน false ทั้งคู่ การ Refresh/RequestScriptCompilation(CleanBuildCache) อย่างเดียวไม่พอ หลังผู้ใช้เซฟและปิด Unity ตรวจ process ปิดจริงแล้วลบเฉพาะ cache ที่สร้างใหม่ได้ใน Library: Bee, ScriptAssemblies, PlayerDataCache, SourceAssetDB/lock, ArtifactDB/lock ไม่แตะ Assets/ProjectSettings/เซฟเกม

เปิด Unity batchmode ครั้งเดียวเพื่อสร้างฐานข้อมูลใหม่และยืนยัน Player Build Windows จริง: ไม่พบ importer ของสอง DLL แล้ว, Build Finished Result Success, result=Succeeded errors=0 warnings=13, ระยะ 00:02:44.2490096; rsp/inputdata/dag ใหม่ไม่มี reference สอง DLL Warning ที่เหลือเป็น shader pow ค่าติดลบ/หารศูนย์และ shader ที่ไม่รองรับ pipeline ไม่ใช่ Error ที่ขวาง Build ไม่แก้กราฟภาพแทนผู้ใช้ ผลทดสอบไม่ใช่การตรวจภาพหรือเล่นเกมจริง

ผู้ใช้แจ้งว่าจะ Build เอง จึงไม่สั่ง Build เพิ่ม เก็บกวาด RemoveMissingTestPlugins/VerifyPlayerBuild scripts และ .meta พร้อมผล/Log ชั่วคราวใน Temp แล้ว ไม่วาง DLL ทดแทน ไม่แก้โค้ดเกม/UI/prefab เพื่อซ่อน Error ไม่มีช่องใหม่ ไม่ต้องตั้งค่า reference เพิ่ม ผู้ใช้เปิด Unity แล้ว Build ในโฟลเดอร์ของตนได้ตามเดิม
