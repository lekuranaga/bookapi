-- More demo books for the seeded user. Idempotent via ON CONFLICT.
INSERT INTO books (id, user_id, title, author, rating, review, read_at, created_at, updated_at)
VALUES
    ('22222222-2222-2222-2222-222222222223', '11111111-1111-1111-1111-111111111111',
     'Domain-Driven Design', 'Eric Evans', 5,
     'Foundational vocabulary for modeling complex domains.',
     CURRENT_DATE - INTERVAL '90 days', NOW(), NOW()),
    ('22222222-2222-2222-2222-222222222224', '11111111-1111-1111-1111-111111111111',
     'Refactoring', 'Martin Fowler', 4,
     'Catalog of code smells with safe transformation recipes.',
     CURRENT_DATE - INTERVAL '60 days', NOW(), NOW()),
    ('22222222-2222-2222-2222-222222222225', '11111111-1111-1111-1111-111111111111',
     'The Mythical Man-Month', 'Fred Brooks', 5,
     'Why adding people to a late project makes it later.',
     CURRENT_DATE - INTERVAL '120 days', NOW(), NOW()),
    ('22222222-2222-2222-2222-222222222226', '11111111-1111-1111-1111-111111111111',
     'Designing Data-Intensive Applications', 'Martin Kleppmann', 5,
     'Best modern reference for distributed data systems.',
     CURRENT_DATE - INTERVAL '20 days', NOW(), NOW()),
    ('22222222-2222-2222-2222-222222222227', '11111111-1111-1111-1111-111111111111',
     'Working Effectively with Legacy Code', 'Michael Feathers', 4,
     'Practical strategies for taming untested codebases.',
     CURRENT_DATE - INTERVAL '45 days', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;
