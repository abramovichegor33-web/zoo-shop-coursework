CREATE DATABASE IF NOT EXISTS zoo_shop
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE zoo_shop;

CREATE TABLE roles (
    id   INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE users (
    id        INT AUTO_INCREMENT PRIMARY KEY,
    login     VARCHAR(50)  NOT NULL UNIQUE,
    password  VARCHAR(100) NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    role_id   INT NOT NULL,
    FOREIGN KEY (role_id) REFERENCES roles(id)
);

CREATE TABLE categories (
    id   INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE products (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    name        VARCHAR(150)  NOT NULL,
    category_id INT NOT NULL,
    price       DECIMAL(10,2) NOT NULL CHECK (price >= 0),
    quantity    INT NOT NULL DEFAULT 0 CHECK (quantity >= 0),
    description TEXT,
    FOREIGN KEY (category_id) REFERENCES categories(id)
);

INSERT INTO roles (name) VALUES ('admin'), ('user');

INSERT INTO users (login, password, full_name, role_id) VALUES
('admin', 'admin123', 'Администратор', 1),
('user',  'user123',  'Покупатель',    2);

INSERT INTO categories (name) VALUES
('Корма для собак'), ('Корма для кошек'), ('Игрушки'), ('Аквариумистика');

INSERT INTO products (name, category_id, price, quantity, description) VALUES
('Сухой корм для собак 2 кг', 1, 850.00, 25, 'Для взрослых собак средних пород'),
('Влажный корм для кошек',    2,  65.50, 120, 'Паучи с курицей'),
('Мячик резиновый',           3, 150.00, 40, 'Для игр с собакой'),
('Аквариум 50 л',             4, 4200.00, 5, 'В комплекте крышка и подсветка');