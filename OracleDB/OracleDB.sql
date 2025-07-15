/*用户*/
create table "USER" (
   user_id       number primary key, -- 用户id
   user_name     varchar(50) not null, -- 名称
   telephone     varchar(20) not null, -- 电话号码
   email         varchar(30), -- 邮箱
   register_time date, -- 注册时间
   points        number, -- 积分
   avatar_url    blob, -- 头像
   gender        varchar(10) check ( gender in ( 'male',
                                          'female',
                                          'unknown' ) ), -- 性别
   birthday      date, -- 出生日期
   profile       clob, -- 简介
   region        varchar(255), -- 所处地区
   password      varchar(30) not null, -- 密码
   role          varchar(20) check ( role in ( 'normal',
                                      'manager' ) )
);

-- 创建 sequence 用于自增
create sequence user_id_seq start with 1 increment by 1;

-- 创建触发器，在插入时自动赋值 user_id
create or replace trigger trg_user_id before
   insert on "USER"
   for each row
begin
   :new.user_id := user_id_seq.nextval;  -- 使用 SEQUENCE 生成用户ID
end;
/

/*帖子*/
create table post (
   post_id          number primary key, -- 帖子id
   post_content     clob, -- 文本
   post_title       varchar(50), -- 标题
   post_time        date, --发布时间
   post_status      varchar(20) check ( post_status in ( 'private',
                                                    'public' ) ), -- 发布状态
   reading_count    number, -- 阅读量
   collection_count number, --收藏量
   like_count       number, --点赞量
   dislike_count    number --点踩量
);

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

/*评论*/
create table "COMMENT" (
   comment_id      number primary key, -- 评论id
   comment_content clob, -- 文本
   comment_time    date, -- 发布时间
   comment_status  varchar(20) check ( comment_status in ( 'private',
                                                          'public' ) ), -- 发布状态
   like_count      number, --点赞量
   dislike_count   number --点踩量
);

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

/*帖子举报*/
create table post_report (
   report_id        number primary key, -- 举报id
   reporter_id      number
      references "USER" ( user_id ), --举报人id
   reported_user_id number
      references "USER" ( user_id ), --被举报人id
   reported_post_id number
      references post ( post_id ), --被举报帖子id
   report_reason    clob, -- 文本
   report_time      date, -- 发布时间
   report_status    varchar(20) check ( report_status in ( 'checking',
                                                        'accepted',
                                                        'rejected' ) ) -- 举报状态
);

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

/* 评论举报表 */
create table comment_report (
   report_id           number primary key, -- 举报id
   reporter_id         number
      references "USER" ( user_id ), -- 举报人id
   reported_user_id    number
      references "USER" ( user_id ), -- 被举报人id
   reported_comment_id number
      references "COMMENT" ( comment_id ), -- 被举报评论id
   report_reason       clob, -- 举报原因
   report_time         date, -- 举报时间
   report_status       varchar(20) check ( report_status in ( 'checking',
                                                        'accepted',
                                                        'rejected' ) ) -- 举报状态
);

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


/* 开放时间段表 */
create table time_slot (
   time_slot_id number primary key, -- 开放时间段id
   begin_time   date, -- 时段起始时间
   end_time     date  -- 时段结束时间
);

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


/* 预约表 */
create table appointment (
   appointment_id     number primary key, -- 预约id
   appointment_status varchar(20) check ( appointment_status in ( 'upcoming',
                                                                  'ongoing',
                                                                  'canceled',
                                                                  'overtime',
                                                                  'completed' ) ), -- 预约状态
   apply_time         date, -- 申请时间
   finish_time        date, -- 实际结束时间
   begin_time         date, -- 预约时段起点
   end_time           date -- 预约结束时间
);

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


/* 违约记录表 */
create table violation (
   violation_id      number primary key, -- 违约id
   appointment_id    number
      references appointment ( appointment_id ), -- 预约id
   violation_reason  clob, -- 违约原因
   violation_time    date, -- 违约时间
   violation_penalty clob -- 处罚措施
);

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


/* 场地表 */
create table venue (
   venue_id       number primary key, -- 场地id
   venue_name     varchar(20) not null, -- 场地名称
   venue_type     varchar(20) not null, -- 场地类型
   venue_location varchar(50) not null, -- 场地地点
   venue_capacity number, -- 场地容量
   venue_status   varchar(20) check ( venue_status in ( 'open',
                                                      'close' ) ) -- 发布状态
);

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


/* 积分变化表 */
create table point_change (
   change_id     number primary key, -- 变化id
   user_id       number
      references "USER" ( user_id ), -- 用户id
   change_amount number, -- 变化数量
   change_time   date, -- 变化时间
   change_reason clob -- 变化原因
);

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


/* 维护记录表 */
create table maintenance (
   maintenance_id      number primary key, -- 维护id
   user_id             number
      references "USER" ( user_id ), -- 用户id
   venue_id            number
      references venue ( venue_id ), -- 场地id
   maintenance_time    date, -- 维护时间
   maintenance_content clob -- 维护内容
);

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


/* 账单表 */
create table bill (
   bill_id        number primary key, -- 账单id
   bill_status    varchar(20) check ( bill_status in ( 'completed',
                                                    'pending' ) ), -- 支付状态
   bill_amount    number, -- 账单金额
   begin_time     date, -- 账单创建时间
   user_id        number
      references "USER" ( user_id ), -- 用户id
   appointment_id number
      references appointment ( appointment_id ) -- 预约id
);

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


------------------------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------------------------


/*开放时间段-场地关联表*/
create table venue_time_slot (
   time_slot_id     number
      references time_slot ( time_slot_id ),
   venue_id         number
      references venue ( venue_id ),
   actual_number    number,
   time_slot_status varchar(20) check ( time_slot_status in ( 'available',
                                                              'busy' ) ),
   primary key ( time_slot_id,
                 venue_id )
);

/*场地-预约：场地预约记录表*/
create table venue_appointment (
   appointment_id number
      references appointment ( appointment_id ),
   venue_id       number
      references venue ( venue_id ),
   primary key ( appointment_id )
);

/*场地-管理员：管理员职责记录表*/
create table venue_manager (
   manager_id number
      references "USER" ( user_id ),
   venue_id   number
      references venue ( venue_id ),
   primary key ( manager_id,
                 venue_id )
);

/*用户-预约：用户预约记录与结果通知表*/
create table user_appointment (
   user_id        number
      references "USER" ( user_id ),
   appointment_id number
      references appointment ( appointment_id ),
   primary key ( appointment_id )
);

/*用户-违约记录关联表*/
create table user_violation (
   user_id      number
      references "USER" ( user_id ),
   violation_id number
      references violation ( violation_id ),
   primary key ( violation_id,
                 user_id )
);

/*用户（管理员）-举报：帖子举报审核表*/
create table manager_post_report (
   manager_id    number
      references "USER" ( user_id ),
   report_id     number
      references post_report ( report_id ),
   manage_time   date,
   manage_reason clob,
   primary key ( report_id )
);

/*用户（管理员）-举报：评论举报审核表*/
create table manager_comment_report (
   manager_id    number
      references "USER" ( user_id ),
   report_id     number
      references comment_report ( report_id ),
   manage_time   date,
   manage_reason clob,
   primary key ( report_id )
);

/*用户-用户（普通用户-管理员）：黑名单表*/
create table blacklist (
   user_id       number
      references "USER" ( user_id ),
   manager_id    number
      references "USER" ( user_id ),
   begin_time    date not null,
   end_time      date not null,
   banned_reason clob,
   banned_status varchar(20) check ( banned_status in ( 'valid',
                                                        'invalid' ) ),
   primary key ( user_id )
);

/*用户-帖子：用户发帖记录表*/
create table user_post (
   user_id number
      references "USER" ( user_id ),
   post_id number
      references post ( post_id ),
   primary key ( post_id )
);

/*用户-帖子：用户点赞记录表*/
create table post_like (
   user_id   number
      references "USER" ( user_id ),
   post_id   number
      references post ( post_id ),
   like_time date not null,
   primary key ( user_id,
                 post_id )
);

/*用户-帖子：用户点踩赞记录表*/
create table post_dislike (
   user_id      number
      references "USER" ( user_id ),
   post_id      number
      references post ( post_id ),
   dislike_time date not null,
   primary key ( user_id,
                 post_id )
);

/*用户-帖子：用户收藏记录表*/
create table post_collection (
   user_id      number
      references "USER" ( user_id ),
   post_id      number
      references post ( post_id ),
   collect_time date not null,
   primary key ( user_id,
                 post_id )
);

/*用户-评论：用户评论记录表*/
create table user_comment (
   user_id    number
      references "USER" ( user_id ),
   comment_id number
      references "COMMENT" ( comment_id ),
   primary key ( comment_id )
);

/*用户-评论：用户评论点赞记录表*/
create table comment_like (
   user_id    number
      references "USER" ( user_id ),
   comment_id number
      references "COMMENT" ( comment_id ),
   like_time  date not null,
   primary key ( user_id,
                 comment_id )
);

/*用户-评论：用户评论点踩赞记录表*/
create table comment_dislike (
   user_id      number
      references "USER" ( user_id ),
   comment_id   number
      references "COMMENT" ( comment_id ),
   dislike_time date not null,
   primary key ( user_id,
                 comment_id )
);

/*评论-评论：评论回复记录表*/
create table comment_reply (
   comment_id number
      references "COMMENT" ( comment_id ),
   reply_id   number
      references "COMMENT" ( comment_id ),
   primary key ( reply_id )
);

/*帖子-评论：帖子评论记录*/
create table post_comment (
   post_id    number
      references post ( post_id ),
   comment_id number
      references "COMMENT" ( comment_id ),
   primary key ( comment_id )
);

/*签到*/
create table checkin (
   appointment_id number
      references appointment ( appointment_id )
   primary key, --预约id
   user_id        number
      references "USER" ( user_id ), -- 用户id
   checkin_time   date --签到时间
);

commit;