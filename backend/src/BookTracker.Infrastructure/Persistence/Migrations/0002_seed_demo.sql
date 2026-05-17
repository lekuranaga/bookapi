-- Demo user: demo@demo.com / Demo@123
INSERT INTO users (id, email, password_hash, created_at)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    'demo@demo.com',
    '$2a$12$Y5tcjfUDMBJPH1korPHpV.R39ZgkzH6KTtmvgeZ61i/Go6CRKxj/q',
    NOW()
)
ON CONFLICT (email) DO NOTHING;

INSERT INTO books (id, user_id, title, author, rating, review, read_at, created_at, updated_at)
VALUES
    ('22222222-2222-2222-2222-222222222221', '11111111-1111-1111-1111-111111111111',
     'Clean Code', 'Robert C. Martin', 5, 'Foundational.', CURRENT_DATE - INTERVAL '30 days', NOW(), NOW()),
    ('22222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111',
     'The Pragmatic Programmer', 'Andy Hunt & Dave Thomas', 5, 'Timeless.', CURRENT_DATE - INTERVAL '10 days', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;
