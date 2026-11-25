INSERT INTO portfolio.course (code, title, school_year, semester) VALUES
('PROGIT1', 'Programming 1', '2025-2026', '1st semester'),
('DBMWEB1', 'Database Management and Web Systems', '2026-2027', '1st semester');

INSERT INTO portfolio.project (course_id, title, summary, video_url, completed_on)
SELECT course_id, 'Library System',
       'An ASP.NET Core MVC application on PostgreSQL: books, members and loans, with validation, error handling and a borrowing transaction.',
       NULL, DATE '2026-11-12'
FROM portfolio.course WHERE code = 'DBMWEB1';