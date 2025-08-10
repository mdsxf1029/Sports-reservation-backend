-- 创建 sequence 用于user_id自增
create sequence user_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 user_id
create or replace trigger trg_user_id before
   insert on "USER"
   for each row
begin
   :new.user_id := user_id_seq.nextval;  -- 使用 SEQUENCE 生成用户ID
end;
/

--自动调整注册时间
create or replace trigger set_register_time before
   insert on "USER"
   for each row
begin
   if :new.register_time is null then
      :new.register_time := sysdate;
   end if;
end;
/

--创建 sequence 用于post_id自增
create sequence post_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 post_id
create or replace trigger trg_post_id before
   insert on post
   for each row
begin
   :new.post_id := post_id_seq.nextval;
end;
/

-- 创建 sequence 用于 comment_id 自增
create sequence comment_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 comment_id
create or replace trigger trg_comment_id before
   insert on "COMMENT"
   for each row
begin
   :new.comment_id := comment_id_seq.nextval;
end;
/

-- 创建 SEQUENCE 用于 report_id 自增
create sequence report_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 report_id
create or replace trigger trg_report_id before
   insert on post_report
   for each row
begin
   -- 使用 NEXTVAL 获取序列的下一个值
   :new.report_id := report_id_seq.nextval;
end;
/

-- 创建 SEQUENCE 用于 report_id 自增
create sequence comment_report_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 report_id
create or replace trigger trg_comment_report_id before
   insert on comment_report
   for each row
begin
   :new.report_id := comment_report_id_seq.nextval;
end;
/

-- 创建 SEQUENCE 用于 time_slot_id 自增
create sequence time_slot_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 time_slot_id
create or replace trigger trg_time_slot_id before
   insert on time_slot
   for each row
begin
   :new.time_slot_id := time_slot_id_seq.nextval;
end;
/

-- 创建 SEQUENCE 用于 appointment_id 自增
create sequence appointment_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 appointment_id
create or replace trigger trg_appointment_id before
   insert on appointment
   for each row
begin
   :new.appointment_id := appointment_id_seq.nextval;
end;
/

-- 创建 SEQUENCE 用于 violation_id 自增
create sequence violation_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 violation_id
create or replace trigger trg_violation_id before
   insert on violation
   for each row
begin
   :new.violation_id := violation_id_seq.nextval;
end;
/

-- 创建 SEQUENCE 用于 venue_id 自增
create sequence venue_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 venue_id
create or replace trigger trg_venue_id before
   insert on venue
   for each row
begin
   :new.venue_id := venue_id_seq.nextval;
end;
/

-- 创建 SEQUENCE 用于 change_id 自增
create sequence point_change_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 change_id
create or replace trigger trg_point_change_id before
   insert on point_change
   for each row
begin
   :new.change_id := point_change_id_seq.nextval;
end;
/

-- 创建 SEQUENCE 用于 maintenance_id 自增
create sequence maintenance_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 maintenance_id
create or replace trigger trg_maintenance_id before
   insert on maintenance
   for each row
begin
   :new.maintenance_id := maintenance_id_seq.nextval;
end;
/

-- 创建 SEQUENCE 用于 bill_id 自增
create sequence bill_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 bill_id
create or replace trigger trg_bill_id before
   insert on bill
   for each row
begin
   :new.bill_id := bill_id_seq.nextval;
end;
/