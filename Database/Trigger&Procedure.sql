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
      :new.register_time := ( systimestamp + interval '8' hour );
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
-- 创建触发器，在插入时自动将其与所有timeslot关联
create or replace trigger trg_venue_after_insert after
   insert on venue
   for each row
begin
   insert into venue_time_slot (
      time_slot_id,
      venue_id,
      actual_number,
      time_slot_status
   )
      select t.time_slot_id,
             :new.venue_id,
             0,
             'available'
        from time_slot t;
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








-- 创建 SEQUENCE 用于 notification_id 自增
create sequence notification_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 notification_id
create or replace trigger trg_notification_id before
   insert on notification
   for each row
begin
   :new.notification_id := notification_id_seq.nextval;
end;
/

--自动调整notification创建时间
create or replace trigger set_notification_time before
   insert on notification
   for each row
begin
   if :new.createtime is null then
      :new.createtime := ( systimestamp + interval '8' hour );
   end if;
end;
/


--创建SEQUENCE 用于appeal_id自增
create sequence appeal_id_seq start with 1 increment by 1;

--创建触发器，在插入时自动赋值 appeal_id
create or replace trigger trg_appeal_id before
   insert on appeal
   for each row
begin
   :new.appeal_id := appeal_id_seq.nextval;
end;
/

--自动调整appeal创建时间
create or replace trigger set_appeal_time before
   insert on appeal
   for each row
begin
   if :new.appeal_time is null then
      :new.appeal_time := ( systimestamp + interval '8' hour );
   end if;
end;
/

-- 更新预约状态
create or replace procedure update_appointment_status as
begin
    -- 检查所有已过期的 "upcoming" 状态的预约，并将其更改为 "overtime"
   for rec in (
      select appointment_id,
             appointment_status,
             end_time
        from appointment
       where end_time < ( systimestamp + interval '8' hour )
         and appointment_status = 'upcoming'
   ) loop
      update appointment
         set
         appointment_status = 'overtime'
       where appointment_id = rec.appointment_id;
   end loop;

    -- 检查所有已过期的 "ongoing" 状态的预约，并将其更改为 "completed"
   for rec in (
      select appointment_id,
             appointment_status,
             end_time
        from appointment
       where end_time < ( systimestamp + interval '8' hour )
         and appointment_status = 'ongoing'
   ) loop
      update appointment
         set
         appointment_status = 'completed'
       where appointment_id = rec.appointment_id;
   end loop;
end update_appointment_status;
/


-- 定时更新预约状态
begin
   dbms_scheduler.create_job(
      job_name        => 'update_appointment_status_job',  -- 定时任务名称
      job_type        => 'PLSQL_BLOCK',  -- 作业类型
      job_action      => 'BEGIN update_appointment_status; END;',  -- 执行的存储过程
      start_date      => systimestamp,  -- 定时任务开始时间
      repeat_interval => 'FREQ=MINUTELY; INTERVAL=1',  -- 每 1 分钟执行一次
      enabled         => true,  -- 启用任务
      comments        => '每分钟检查并更新预约的状态'
   );
end;
/

-- overtime对应过程
create or replace procedure handle_overtime_violation (
   p_appointment_id in number
) as
   v_user_id      number;
   v_points       number;
   v_violation_id number;
begin
    -- 1. 获取 user_id
   select ua.user_id
     into v_user_id
     from user_appointment ua
    where ua.appointment_id = p_appointment_id;
    
    -- 2. 获取当前积分
   select u.points
     into v_points
     from "USER" u
    where u.user_id = v_user_id;

    -- 3. 扣除积分
   update "USER"
      set
      points = points - 10
    where user_id = v_user_id;

    -- 4. 在 point_change 表中插入一条记录
   insert into point_change (
      change_id,
      user_id,
      change_amount,
      change_time,
      change_reason
   ) values ( point_change_id_seq.nextval,
              v_user_id,
              - 10,
              ( systimestamp + interval '8' hour ),
              '预约未签到' );
    
    -- 5. 在 violation 表中插入一条记录
   insert into violation (
      violation_id,
      appointment_id,
      violation_reason,
      violation_time,
      violation_penalty
   ) values ( violation_id_seq.nextval,
              p_appointment_id,
              '预约未签到',
              ( systimestamp + interval '8' hour ),
              '积分-10' );
    
    -- 获取生成的 violation_id
   select violation_id_seq.currval
     into v_violation_id
     from dual;
    
    -- 6. 在 user_violation 表中插入一条记录
   insert into user_violation (
      user_id,
      violation_id
   ) values ( v_user_id,
              v_violation_id );
    
    -- 输出成功信息
   dbms_output.put_line('处理完成，预约已变为 oveltime，用户积分扣除 10 分');
exception
   when others then
      dbms_output.put_line('处理失败：' || sqlerrm);
end handle_overtime_violation;
/

--当修改为overtime时使用触发器调用过程
create or replace trigger trg_update_overtime after
   update of appointment_status on appointment
   for each row
   when ( new.appointment_status = 'overtime'
      and old.appointment_status != 'overtime' ) -- 仅在状态变化时触发
begin
    -- 调用存储过程
   handle_overtime_violation(:new.appointment_id);
end trg_update_overtime;
/

--completed对应过程
create or replace procedure handle_completed_appointment (
   p_appointment_id in number
) as
   v_user_id number;
   v_points  number;
begin
    -- 1. 获取 user_id
   select ua.user_id
     into v_user_id
     from user_appointment ua
    where ua.appointment_id = p_appointment_id;
    
    -- 2. 获取当前积分
   select u.points
     into v_points
     from "USER" u
    where u.user_id = v_user_id;

    -- 3. 增加积分
   update "USER"
      set
      points = points + 10
    where user_id = v_user_id;

    -- 4. 在 point_change 表中插入一条记录
   insert into point_change (
      change_id,
      user_id,
      change_amount,
      change_time,
      change_reason
   ) values ( point_change_id_seq.nextval,
              v_user_id,
              10,
              ( systimestamp + interval '8' hour ),
              '预约签到' );
    
    -- 输出成功信息
   dbms_output.put_line('处理完成，预约已变为 completed，用户积分增加 10 分');
exception
   when others then
        -- 错误处理
      dbms_output.put_line('处理失败：' || sqlerrm);
end handle_completed_appointment;
/


--当修改为completed使用触发器调用过程
create or replace trigger trg_update_completed after
   update of appointment_status on appointment
   for each row
   when ( new.appointment_status = 'completed'
      and old.appointment_status != 'completed' ) -- 仅在状态变更时触发
begin
    -- 调用存储过程
   handle_completed_appointment(:new.appointment_id);
end trg_update_completed;
/


-- 1. 创建一个存储过程来更新过期的黑名单
create or replace procedure update_expired_blacklist is
begin
   update blacklist
      set
      banned_status = 'invalid'
    where banned_status = 'valid'
      and endtime <= ( systimestamp + interval '8' hour );

   commit;
end;
/

-- 2. 创建一个 Scheduler Job，每分钟执行一次
begin
   dbms_scheduler.create_job(
      job_name        => 'CHECK_BLACKLIST_EXPIRY',
      job_type        => 'STORED_PROCEDURE',
      job_action      => 'UPDATE_EXPIRED_BLACKLIST',
      start_date      => systimestamp,
      repeat_interval => 'FREQ=MINUTELY; INTERVAL=1', -- 每分钟执行
      enabled         => true,
      comments        => '每分钟检查一次黑名单是否过期'
   );
end;
/


-- 1. 创建一个存储过程来更新过期的黑名单
create or replace procedure update_expired_blacklist is
begin
   update blacklist
      set
      banned_status = 'invalid'
    where banned_status = 'valid'
      and end_time <= ( systimestamp + interval '8' hour ); -- 注意时区！！

   commit;
end;
/

-- 2. 创建一个 Scheduler Job，每分钟执行一次
begin
   dbms_scheduler.create_job(
      job_name        => 'CHECK_BLACKLIST_EXPIRY',
      job_type        => 'STORED_PROCEDURE',
      job_action      => 'UPDATE_EXPIRED_BLACKLIST',
      start_date      => systimestamp,
      repeat_interval => 'FREQ=MINUTELY; INTERVAL=1', -- 每分钟执行
      enabled         => true,
      comments        => '每分钟检查一次黑名单是否过期'
   );
end;
/