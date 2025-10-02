INSERT INTO images (name, url, publication_year)
VALUES
  ('Sample Image 1', 'imagem 1 url', '2022-01-01'),
  ('Sample Image 2', 'imagem 2 url', '2023-02-15'),
  ('Sample Image 3', 'imagem 3 url', '2024-03-20')
ON CONFLICT DO NOTHING;
