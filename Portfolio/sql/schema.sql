DROP SCHEMA IF EXISTS portfolio CASCADE;

CREATE SCHEMA portfolio;

CREATE TABLE portfolio.course (
    course_id   BIGINT GENERATED ALWAYS AS IDENTITY,
    code        VARCHAR(12)  NOT NULL,
    title       VARCHAR(80)  NOT NULL,
    school_year VARCHAR(9)   NOT NULL,
    semester    VARCHAR(12)  NOT NULL,
    CONSTRAINT pk_course PRIMARY KEY (course_id),
    CONSTRAINT uq_course_code UNIQUE (code),
    CONSTRAINT ck_course_semester CHECK (semester IN ('1st semester', '2nd semester', 'Summer'))
);

CREATE TABLE portfolio.project (
    project_id   BIGINT GENERATED ALWAYS AS IDENTITY,
    course_id    BIGINT       NOT NULL,
    title        VARCHAR(100) NOT NULL,
    summary      VARCHAR(500),
    video_url    VARCHAR(200),
    completed_on DATE,
    CONSTRAINT pk_project PRIMARY KEY (project_id),
    CONSTRAINT uq_project_title UNIQUE (course_id, title),
    CONSTRAINT fk_project_course FOREIGN KEY (course_id)
        REFERENCES portfolio.course (course_id) ON DELETE RESTRICT
);

CREATE TABLE portfolio.project_image (
    project_id    BIGINT       NOT NULL,
    image_url     VARCHAR(300) NOT NULL,
    display_order INT          NOT NULL,
    CONSTRAINT pk_project_image PRIMARY KEY (project_id, image_url),
    CONSTRAINT ck_image_order CHECK (display_order >= 1),
    CONSTRAINT fk_image_project FOREIGN KEY (project_id)
        REFERENCES portfolio.project (project_id) ON DELETE CASCADE
);