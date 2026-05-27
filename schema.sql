-- Users (students + admins)
CREATE TABLE users (
    id                        INT PRIMARY KEY AUTO_INCREMENT,
    full_name                 VARCHAR(100) NOT NULL,
    email                     VARCHAR(150) NOT NULL UNIQUE,
    password_hash             VARCHAR(255) NOT NULL,
    phone_number              VARCHAR(20),
    role                      ENUM('student', 'admin') NOT NULL DEFAULT 'student',
    university_id             VARCHAR(50),                      -- YU student ID
    taking_volunteering_course BOOLEAN NOT NULL DEFAULT FALSE,
    profile_picture_url       VARCHAR(500),
    created_at                TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Organizations that post tasks
CREATE TABLE organizations (
    id                INT PRIMARY KEY AUTO_INCREMENT,
    name              VARCHAR(150) NOT NULL,
    email             VARCHAR(150) NOT NULL UNIQUE,
    password_hash     VARCHAR(255) NOT NULL,
    phone_number      VARCHAR(20),
    logo_url          VARCHAR(500),
    description       VARCHAR(1000),
    created_at        TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Volunteering tasks
CREATE TABLE tasks (
    id               INT PRIMARY KEY AUTO_INCREMENT,
    organization_id  INT NOT NULL,
    title            VARCHAR(200) NOT NULL,
    description      TEXT,
    max_volunteers   INT,                                -- NULL = unlimited
    status           ENUM('open', 'closed', 'done') NOT NULL DEFAULT 'open',
    start_date       TIMESTAMP,
    end_date         TIMESTAMP,
    created_at       TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (organization_id) REFERENCES organizations(id)
);

-- Which students signed up for which task
CREATE TABLE task_volunteers (
    id          INT PRIMARY KEY AUTO_INCREMENT,
    task_id     INT NOT NULL,
    user_id     INT NOT NULL,
    status      ENUM('pending', 'approved', 'rejected') DEFAULT 'pending',
    joined_at   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (task_id, user_id),                          -- no duplicate signups
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (user_id)  REFERENCES users(id)
);

-- Hours each student worked on each task
CREATE TABLE volunteer_hours (
    id           INT PRIMARY KEY AUTO_INCREMENT,
    task_id      INT NOT NULL,
    user_id      INT NOT NULL,
    hours_worked DECIMAL(5,2) NOT NULL,                 -- e.g. 2.50 = 2h 30m
    notes        VARCHAR(500),
    recorded_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (user_id)  REFERENCES users(id)
);

-- Images attached to tasks
CREATE TABLE task_images (
    id           INT PRIMARY KEY AUTO_INCREMENT,
    task_id      INT NOT NULL,
    image_url    VARCHAR(500) NOT NULL,
    uploaded_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id)
);

-- Tag library (e.g. "health", "education", "environment")
CREATE TABLE tags (
    id   INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(50) NOT NULL UNIQUE
);

-- Many-to-many: tasks ↔ tags
CREATE TABLE task_tags (
    task_id INT NOT NULL,
    tag_id  INT NOT NULL,
    PRIMARY KEY (task_id, tag_id),
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (tag_id)  REFERENCES tags(id)
);

-- Reviews / ratings for organizations
CREATE TABLE organization_reviews (
    id                INT PRIMARY KEY AUTO_INCREMENT,
    organization_id   INT NOT NULL,
    user_id           INT NOT NULL,
    rating            INT NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment           VARCHAR(1000),
    created_at        TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (organization_id, user_id),
    FOREIGN KEY (organization_id) REFERENCES organizations(id),
    FOREIGN KEY (user_id)          REFERENCES users(id)
);