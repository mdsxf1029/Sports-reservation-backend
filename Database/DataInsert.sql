/*插入30天timeslot数据*/
declare
   v_start_date date := to_date ( '2025-08-25','YYYY-MM-DD' ); -- 起始日期
   v_day        number;
   v_hour       number;
   v_id         number;
begin
   for v_day in 0..29 loop  -- 30 天
      for v_hour in 8..19 loop -- 每天 8:00 到 20:00（19表示19:00-20:00）
         select nvl(
            max(time_slot_id),
            0
         ) + 1
           into v_id
           from time_slot;

         insert into time_slot (
            time_slot_id,
            begin_time,
            end_time
         ) values ( v_id,
                    v_start_date + v_day + ( v_hour / 24 ),
                    v_start_date + v_day + ( ( v_hour + 1 ) / 24 ) );

      -- 插入 venue_time_slot，关联所有场地
         insert into venue_time_slot (
            time_slot_id,
            venue_id,
            actual_number,
            time_slot_status
         )
            select v_id,
                   v.venue_id,
                   0,
                   'available'
              from venue v;
      end loop;
   end loop;
   commit;
end;
/