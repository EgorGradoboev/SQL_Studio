-- SQL Studio test data (PostgreSQL).
--
-- Adds ~300 customers, 60 products, ~1500 orders, ~3500 order items to the existing tables
-- and creates new tables: suppliers, product_suppliers, employees, payments, shipments, reviews.
--
-- Safe to re-run: rows added by this script are recognised by markers
-- (customers: email '%@seed.example', products: sku 'SEED-%') and removed first.
-- Your original rows are never modified or deleted.
-- The new tables (suppliers, product_suppliers, employees, payments, shipments, reviews)
-- are dropped and recreated on every run.
--
-- The whole script runs in one transaction: if anything fails, nothing is changed.

BEGIN;

SELECT setseed(0.42);

-- ---------------------------------------------------------------------------
-- 0. Clean up a previous run
-- ---------------------------------------------------------------------------
DROP TABLE IF EXISTS reviews, payments, shipments, product_suppliers, suppliers, employees;

DELETE FROM order_items
WHERE order_id IN (SELECT o.id FROM orders o JOIN customers c ON c.id = o.customer_id
                   WHERE c.email LIKE '%@seed.example')
   OR product_id IN (SELECT id FROM products WHERE sku LIKE 'SEED-%');
DELETE FROM orders WHERE customer_id IN (SELECT id FROM customers WHERE email LIKE '%@seed.example');
DELETE FROM customers WHERE email LIKE '%@seed.example';
DELETE FROM products WHERE sku LIKE 'SEED-%';

-- ---------------------------------------------------------------------------
-- 1. Categories (only the ones that do not exist yet)
-- ---------------------------------------------------------------------------
INSERT INTO categories (id, name) OVERRIDING SYSTEM VALUE
SELECT m.max_id + row_number() OVER (), n.name
FROM (SELECT COALESCE(MAX(id), 0) AS max_id FROM categories) m
CROSS JOIN unnest(ARRAY['Электроника', 'Одежда', 'Книги', 'Дом и сад',
                        'Спорт', 'Игрушки', 'Продукты', 'Красота']) AS n(name)
WHERE NOT EXISTS (SELECT 1 FROM categories c WHERE c.name = n.name);

-- ---------------------------------------------------------------------------
-- 2. Customers (300; ~15% without phone; Moscow and Saint Petersburg dominate)
-- ---------------------------------------------------------------------------
INSERT INTO customers (id, email, full_name, phone, city, created_at) OVERRIDING SYSTEM VALUE
SELECT m.max_id + g,
       'user' || (m.max_id + g) || '@seed.example',
       CASE WHEN g % 2 = 0
            THEN (ARRAY['Иван', 'Алексей', 'Дмитрий', 'Сергей', 'Андрей',
                        'Михаил', 'Николай', 'Павел', 'Артём', 'Максим'])[1 + floor(random() * 10)::int]
                 || ' ' ||
                 (ARRAY['Иванов', 'Петров', 'Смирнов', 'Кузнецов', 'Попов',
                        'Соколов', 'Лебедев', 'Козлов', 'Новиков', 'Морозов'])[1 + floor(random() * 10)::int]
            ELSE (ARRAY['Анна', 'Мария', 'Елена', 'Ольга', 'Наталья',
                        'Татьяна', 'Екатерина', 'Ирина', 'Юлия', 'Светлана'])[1 + floor(random() * 10)::int]
                 || ' ' ||
                 (ARRAY['Иванов', 'Петров', 'Смирнов', 'Кузнецов', 'Попов',
                        'Соколов', 'Лебедев', 'Козлов', 'Новиков', 'Морозов'])[1 + floor(random() * 10)::int] || 'а'
       END,
       CASE WHEN random() < 0.15 THEN NULL
            ELSE '+7 9' || lpad(floor(random() * 100)::int::text, 2, '0')
                 || ' ' || lpad(floor(random() * 1000)::int::text, 3, '0')
                 || '-' || lpad(floor(random() * 100)::int::text, 2, '0')
                 || '-' || lpad(floor(random() * 100)::int::text, 2, '0')
       END,
       (ARRAY['Москва', 'Санкт-Петербург', 'Казань', 'Новосибирск', 'Екатеринбург',
              'Нижний Новгород', 'Самара', 'Краснодар', 'Воронеж', 'Ростов-на-Дону'])
           [1 + floor(power(random(), 1.7) * 10)::int],
       date_trunc('second', LOCALTIMESTAMP - random() * interval '730 days')
FROM (SELECT COALESCE(MAX(id), 0) AS max_id FROM customers) m
CROSS JOIN generate_series(1, 300) g;

-- ---------------------------------------------------------------------------
-- 3. Products (60; some out of stock, some inactive; the last 10 are never sold)
-- ---------------------------------------------------------------------------
INSERT INTO products (id, category_id, sku, name, price, stock_quantity, is_active) OVERRIDING SYSTEM VALUE
SELECT m.max_id + p.ord,
       c.id,
       'SEED-' || lpad(p.ord::text, 4, '0'),
       p.name,
       p.price,
       CASE WHEN random() < 0.08 THEN 0 ELSE floor(random() * 400)::int END,
       random() > 0.1
FROM (SELECT COALESCE(MAX(id), 0) AS max_id FROM products) m
CROSS JOIN (VALUES
    (1,  'Смартфон Nova X',                 'Электроника', 29990.00),
    (2,  'Ноутбук Air 14',                  'Электроника', 64990.00),
    (3,  'Наушники беспроводные',           'Электроника',  4990.00),
    (4,  'Умные часы Fit 7',                'Электроника',  7990.00),
    (5,  'Планшет Tab 10',                  'Электроника', 19990.00),
    (6,  'Power bank 20000 мАч',            'Электроника',  2490.00),
    (7,  'Электрочайник',                   'Электроника',  1990.00),
    (8,  'Монитор 27 дюймов',               'Электроника', 15990.00),
    (9,  'Клавиатура механическая',         'Электроника',  5490.00),
    (10, 'Мышь игровая',                    'Электроника',  2990.00),
    (11, 'Куртка зимняя',                   'Одежда',       8990.00),
    (12, 'Джинсы классические',             'Одежда',       3990.00),
    (13, 'Футболка хлопковая',              'Одежда',        990.00),
    (14, 'Кроссовки беговые',               'Одежда',       5990.00),
    (15, 'Худи оверсайз',                   'Одежда',       2990.00),
    (16, 'Платье летнее',                   'Одежда',       3490.00),
    (17, 'Шарф шерстяной',                  'Одежда',       1290.00),
    (18, 'Рюкзак городской',                'Одежда',       2490.00),
    (19, 'Учебник SQL для начинающих',      'Книги',         890.00),
    (20, 'Детектив «Ночной экспресс»',      'Книги',         450.00),
    (21, 'Фантастика «Орбита»',             'Книги',         520.00),
    (22, 'Кулинарная книга',                'Книги',        1290.00),
    (23, 'Словарь английского языка',       'Книги',         990.00),
    (24, 'Python за 30 дней',               'Книги',        1190.00),
    (25, 'Набор посуды 6 предметов',        'Дом и сад',    3490.00),
    (26, 'Постельное бельё',                'Дом и сад',    2990.00),
    (27, 'Робот-пылесос',                   'Дом и сад',   18990.00),
    (28, 'Лампа настольная',                'Дом и сад',    1490.00),
    (29, 'Горшок для цветов',               'Дом и сад',     390.00),
    (30, 'Газонокосилка',                   'Дом и сад',   12990.00),
    (31, 'Набор инструментов',              'Дом и сад',    4590.00),
    (32, 'Плед флисовый',                   'Дом и сад',    1290.00),
    (33, 'Гантели 2 по 5 кг',               'Спорт',        2490.00),
    (34, 'Коврик для йоги',                 'Спорт',        1290.00),
    (35, 'Велосипед горный',                'Спорт',       24990.00),
    (36, 'Мяч футбольный',                  'Спорт',        1490.00),
    (37, 'Палатка 3-местная',               'Спорт',        6990.00),
    (38, 'Скакалка',                        'Спорт',         390.00),
    (39, 'Спортивная бутылка',              'Спорт',         590.00),
    (40, 'Эспандер',                        'Спорт',         690.00),
    (41, 'Конструктор 500 деталей',         'Игрушки',      3990.00),
    (42, 'Кукла интерактивная',             'Игрушки',      2490.00),
    (43, 'Настольная игра «Экономика»',     'Игрушки',      1990.00),
    (44, 'Пазл 1000 элементов',             'Игрушки',       890.00),
    (45, 'Радиоуправляемая машина',         'Игрушки',      3290.00),
    (46, 'Плюшевый медведь',                'Игрушки',      1190.00),
    (47, 'Кофе в зёрнах 1 кг',              'Продукты',     1490.00),
    (48, 'Чай чёрный 100 пакетиков',        'Продукты',      390.00),
    (49, 'Шоколад тёмный',                  'Продукты',       89.00),
    (50, 'Оливковое масло 0.5 л',           'Продукты',      690.00),
    (51, 'Мёд натуральный',                 'Продукты',      550.00),
    (52, 'Орехи ассорти',                   'Продукты',      490.00),
    (53, 'Макароны',                        'Продукты',       79.00),
    (54, 'Гречка 1 кг',                     'Продукты',      119.00),
    (55, 'Крем для лица',                   'Красота',       890.00),
    (56, 'Шампунь',                         'Красота',       450.00),
    (57, 'Парфюм',                          'Красота',      3990.00),
    (58, 'Набор кистей для макияжа',        'Красота',      1290.00),
    (59, 'Гель для душа',                   'Красота',       299.00),
    (60, 'Маска для волос',                 'Красота',       590.00)
) AS p(ord, name, cat, price)
JOIN (SELECT DISTINCT ON (name) id, name FROM categories ORDER BY name, id) c ON c.name = p.cat;

-- ---------------------------------------------------------------------------
-- 4. Orders (1500; ~60 customers never order; statuses are weighted)
-- ---------------------------------------------------------------------------
WITH sc AS MATERIALIZED (
    SELECT id, city, row_number() OVER (ORDER BY id) AS rn
    FROM customers
    WHERE email LIKE '%@seed.example'
),
x AS MATERIALIZED (
    SELECT g,
           1 + floor(power(random(), 1.4) * 240)::int AS cust_rn,
           random() AS r_status,
           random() AS r_city,
           1 + floor(random() * 10)::int AS city_idx,
           LOCALTIMESTAMP - random() * interval '600 days' AS ts
    FROM generate_series(1, 1500) g
)
INSERT INTO orders (id, customer_id, order_date, status, delivery_city) OVERRIDING SYSTEM VALUE
SELECT m.max_id + row_number() OVER (ORDER BY x.ts),
       sc.id,
       date_trunc('second', x.ts),
       CASE WHEN x.r_status < 0.72 THEN 'Shipped'
            WHEN x.r_status < 0.82 THEN 'Paid'
            WHEN x.r_status < 0.92 THEN 'New'
            ELSE 'Cancelled'
       END,
       CASE WHEN x.r_city < 0.85 THEN sc.city
            ELSE (ARRAY['Москва', 'Санкт-Петербург', 'Казань', 'Новосибирск', 'Екатеринбург',
                        'Нижний Новгород', 'Самара', 'Краснодар', 'Воронеж', 'Ростов-на-Дону'])[x.city_idx]
       END
FROM x
JOIN sc ON sc.rn = x.cust_rn
CROSS JOIN (SELECT COALESCE(MAX(id), 0) AS max_id FROM orders) m;

-- ---------------------------------------------------------------------------
-- 5. Order items (1-5 per order; price_at_order differs from the current price)
-- ---------------------------------------------------------------------------
WITH so AS MATERIALIZED (
    SELECT o.id,
           1 + floor(random() * 5)::int AS n,
           floor(power(random(), 1.5) * 50)::int AS base
    FROM orders o
    JOIN customers c ON c.id = o.customer_id
    WHERE c.email LIKE '%@seed.example'
),
sp AS (
    SELECT id, price, row_number() OVER (ORDER BY id) AS rn
    FROM products
    WHERE sku LIKE 'SEED-%'
),
items AS MATERIALIZED (
    SELECT so.id AS order_id,
           k,
           ((so.base + k * 7) % 50) + 1 AS rn,
           1 + floor(power(random(), 2) * 4)::int AS qty,
           random() AS jitter
    FROM so
    CROSS JOIN LATERAL generate_series(1, so.n) AS k
)
INSERT INTO order_items (id, order_id, product_id, quantity, price_at_order) OVERRIDING SYSTEM VALUE
SELECT m.max_id + row_number() OVER (ORDER BY items.order_id, items.k),
       items.order_id,
       sp.id,
       items.qty,
       round((sp.price * (0.9 + 0.2 * items.jitter))::numeric, 2)
FROM items
JOIN sp ON sp.rn = items.rn
CROSS JOIN (SELECT COALESCE(MAX(id), 0) AS max_id FROM order_items) m;

-- ---------------------------------------------------------------------------
-- 6. New table: suppliers (12) and which supplier delivers which product
-- ---------------------------------------------------------------------------
CREATE TABLE suppliers (
    id            integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    name          text NOT NULL,
    country       text NOT NULL,
    contact_email text,
    rating        numeric(2, 1)
);

INSERT INTO suppliers (name, country, contact_email, rating) VALUES
    ('ТехноПоставка',   'Россия',    'sales@tehnopostavka.example', 4.6),
    ('ГлобалТрейд',     'Китай',     'export@globaltrade.example',  4.1),
    ('Северный текстиль','Россия',   'info@severtex.example',       4.4),
    ('Book Partners',   'Германия',  'orders@bookpartners.example', 4.8),
    ('ДомСтрой',        'Россия',    NULL,                          3.9),
    ('SportLine',       'Турция',    'b2b@sportline.example',       4.2),
    ('ИгроМир',         'Китай',     'sales@igromir.example',       3.7),
    ('ФудОпт',          'Россия',    'zakaz@fudopt.example',        4.5),
    ('Beauty Source',   'Франция',   'trade@beautysource.example',  4.7),
    ('ЭлектроМаркет',   'Корея',     'sales@electromarket.example', 4.3),
    ('Быстрый склад',   'Беларусь',  NULL,                          NULL),
    ('Новый поставщик', 'Казахстан', 'hello@newsupplier.example',   NULL);

CREATE TABLE product_suppliers (
    product_id     integer NOT NULL REFERENCES products (id),
    supplier_id    integer NOT NULL REFERENCES suppliers (id),
    supply_price   numeric(12, 2) NOT NULL,
    lead_time_days integer NOT NULL,
    PRIMARY KEY (product_id, supplier_id)
);

INSERT INTO product_suppliers (product_id, supplier_id, supply_price, lead_time_days)
SELECT sp.id,
       ((sp.rn * 3 + j) % 12) + 1,
       round((sp.price * (0.55 + random() * 0.25))::numeric, 2),
       3 + floor(random() * 25)::int
FROM (SELECT id, price, row_number() OVER (ORDER BY id) AS rn
      FROM products WHERE sku LIKE 'SEED-%') sp
CROSS JOIN generate_series(0, 2) j
WHERE j <= sp.rn % 3;

-- ---------------------------------------------------------------------------
-- 7. New table: employees (30; managers reference themselves; stores may be shared)
-- ---------------------------------------------------------------------------
CREATE TABLE employees (
    id         integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    full_name  text NOT NULL,
    job_title  text NOT NULL,
    store_id   integer REFERENCES stores (str_id),
    manager_id integer REFERENCES employees (id),
    hire_date  date NOT NULL,
    salary     numeric(10, 2) NOT NULL
);

WITH st AS MATERIALIZED (
    SELECT str_id, row_number() OVER (ORDER BY str_id) AS rn FROM stores
),
e AS MATERIALIZED (
    SELECT g,
           random() AS r_salary,
           floor(random() * 2500)::int AS days_ago,
           1 + floor(random() * 10)::int AS first_idx,
           1 + floor(random() * 10)::int AS last_idx
    FROM generate_series(1, 30) g
)
INSERT INTO employees (full_name, job_title, store_id, manager_id, hire_date, salary)
SELECT CASE WHEN e.g % 2 = 0
            THEN (ARRAY['Иван', 'Алексей', 'Дмитрий', 'Сергей', 'Андрей',
                        'Михаил', 'Николай', 'Павел', 'Артём', 'Максим'])[e.first_idx]
                 || ' ' ||
                 (ARRAY['Волков', 'Фёдоров', 'Орлов', 'Макаров', 'Захаров',
                        'Беляев', 'Комаров', 'Ильин', 'Гусев', 'Титов'])[e.last_idx]
            ELSE (ARRAY['Анна', 'Мария', 'Елена', 'Ольга', 'Наталья',
                        'Татьяна', 'Екатерина', 'Ирина', 'Юлия', 'Светлана'])[e.first_idx]
                 || ' ' ||
                 (ARRAY['Волков', 'Фёдоров', 'Орлов', 'Макаров', 'Захаров',
                        'Беляев', 'Комаров', 'Ильин', 'Гусев', 'Титов'])[e.last_idx] || 'а'
       END,
       CASE WHEN e.g <= 5 THEN 'Управляющий'
            ELSE (ARRAY['Продавец-консультант', 'Кассир', 'Кладовщик', 'Администратор зала'])[1 + e.g % 4]
       END,
       st.str_id,
       CASE WHEN e.g <= 5 THEN NULL ELSE 1 + e.g % 5 END,
       CURRENT_DATE - e.days_ago,
       CASE WHEN e.g <= 5 THEN round((120000 + e.r_salary * 80000)::numeric, 2)
            ELSE round((45000 + e.r_salary * 45000)::numeric, 2)
       END
FROM e
LEFT JOIN st ON st.rn = 1 + e.g % (SELECT GREATEST(COUNT(*), 1) FROM stores)
ORDER BY e.g;

-- ---------------------------------------------------------------------------
-- 8. New table: payments (paid orders; some cancelled ones refunded; some pending failed)
-- ---------------------------------------------------------------------------
CREATE TABLE payments (
    id       integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    order_id integer NOT NULL REFERENCES orders (id),
    amount   numeric(12, 2) NOT NULL,
    method   text NOT NULL,
    status   text NOT NULL,
    paid_at  timestamp NOT NULL
);

WITH tot AS (
    SELECT order_id, SUM(quantity * price_at_order) AS total
    FROM order_items
    GROUP BY order_id
),
so AS MATERIALIZED (
    SELECT o.id, o.order_date, o.status, t.total,
           random() AS r_method,
           random() AS r_extra,
           random() * interval '2 hours' AS delay
    FROM orders o
    JOIN customers c ON c.id = o.customer_id
    JOIN tot t ON t.order_id = o.id
    WHERE c.email LIKE '%@seed.example'
)
INSERT INTO payments (order_id, amount, method, status, paid_at)
SELECT id,
       total,
       CASE WHEN r_method < 0.55 THEN 'card'
            WHEN r_method < 0.75 THEN 'sbp'
            WHEN r_method < 0.90 THEN 'cash'
            ELSE 'wallet'
       END,
       CASE WHEN status IN ('Paid', 'Shipped') THEN 'succeeded'
            WHEN status = 'Cancelled' THEN 'refunded'
            ELSE 'failed'
       END,
       date_trunc('second', order_date + delay)
FROM so
WHERE status IN ('Paid', 'Shipped')
   OR (status = 'Cancelled' AND r_extra < 0.5)
   OR (status = 'New' AND r_extra < 0.3)
ORDER BY order_date;

-- ---------------------------------------------------------------------------
-- 9. New table: shipments (shipped and delivered orders)
-- ---------------------------------------------------------------------------
CREATE TABLE shipments (
    id              integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    order_id        integer NOT NULL REFERENCES orders (id),
    carrier         text NOT NULL,
    tracking_number text NOT NULL,
    shipped_at      timestamp NOT NULL,
    delivered_at    timestamp
);

WITH so AS MATERIALIZED (
    SELECT o.id, o.order_date, o.status,
           random() AS r_carrier,
           random() * interval '2 days' AS ship_delay,
           interval '2 days' + random() * interval '5 days' AS transit
    FROM orders o
    JOIN customers c ON c.id = o.customer_id
    WHERE c.email LIKE '%@seed.example'
      AND o.status = 'Shipped'
)
INSERT INTO shipments (order_id, carrier, tracking_number, shipped_at, delivered_at)
SELECT id,
       CASE WHEN r_carrier < 0.40 THEN 'СДЭК'
            WHEN r_carrier < 0.70 THEN 'Почта России'
            WHEN r_carrier < 0.90 THEN 'Boxberry'
            ELSE 'DPD'
       END,
       'TRK' || lpad(id::text, 8, '0'),
       date_trunc('second', LEAST(LOCALTIMESTAMP, order_date + ship_delay)),
       CASE WHEN order_date + ship_delay + transit <= LOCALTIMESTAMP
            THEN date_trunc('second', order_date + ship_delay + transit)
       END
FROM so
ORDER BY order_date;

-- ---------------------------------------------------------------------------
-- 10. New table: reviews (~15% of delivered items; some without a comment)
-- ---------------------------------------------------------------------------
CREATE TABLE reviews (
    id          integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    product_id  integer NOT NULL REFERENCES products (id),
    customer_id integer NOT NULL REFERENCES customers (id),
    rating      integer NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment     text,
    created_at  timestamp NOT NULL
);

WITH src AS MATERIALIZED (
    SELECT oi.product_id,
           o.customer_id,
           s.delivered_at AS order_date,
           random() AS r_rating,
           random() AS r_comment,
           floor(random() * 3)::int AS pick,
           interval '3 days' + random() * interval '20 days' AS review_delay
    FROM order_items oi
    JOIN orders o ON o.id = oi.order_id
    JOIN shipments s ON s.order_id = o.id
    JOIN customers c ON c.id = o.customer_id
    WHERE s.delivered_at IS NOT NULL
      AND c.email LIKE '%@seed.example'
      AND random() < 0.15
),
rated AS (
    SELECT src.*,
           CASE WHEN r_rating < 0.50 THEN 5
                WHEN r_rating < 0.75 THEN 4
                WHEN r_rating < 0.87 THEN 3
                WHEN r_rating < 0.94 THEN 2
                ELSE 1
           END AS rating
    FROM src
)
INSERT INTO reviews (product_id, customer_id, rating, comment, created_at)
SELECT product_id,
       customer_id,
       rating,
       CASE WHEN r_comment < 0.4 THEN NULL
            WHEN rating >= 4 THEN (ARRAY['Отличный товар, всем доволен',
                                         'Качество на высоте, рекомендую',
                                         'Быстрая доставка, всё как в описании'])[1 + pick]
            WHEN rating = 3 THEN (ARRAY['Нормально, но ожидал большего',
                                        'За свою цену приемлемо',
                                        'Есть небольшие недостатки'])[1 + pick]
            ELSE (ARRAY['Не соответствует описанию',
                        'Плохое качество, разочарован',
                        'Пришёл с браком'])[1 + pick]
       END,
       date_trunc('second', LEAST(LOCALTIMESTAMP, order_date + review_delay))
FROM rated
ORDER BY order_date;

-- ---------------------------------------------------------------------------
-- 11. Move sequences past the inserted ids so the app can keep inserting rows
-- ---------------------------------------------------------------------------
DO $$
DECLARE
    r   record;
    seq text;
BEGIN
    FOR r IN SELECT * FROM (VALUES ('customers', 'id'), ('products', 'id'), ('orders', 'id'),
                                   ('order_items', 'id'), ('categories', 'id')) AS v(t, c)
    LOOP
        seq := pg_get_serial_sequence(r.t, r.c);
        IF seq IS NOT NULL THEN
            EXECUTE format('SELECT setval(%L, (SELECT COALESCE(MAX(%I), 1) FROM %I))', seq, r.c, r.t);
        END IF;
    END LOOP;
END
$$;

COMMIT;

ANALYZE;
