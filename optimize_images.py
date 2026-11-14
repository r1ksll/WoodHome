
from PIL import Image
from pathlib import Path
from io import BytesIO

source_dir = Path("wwwroot/img")
output_dir = source_dir / "optimized"
output_dir.mkdir(exist_ok=True)

MAX_SIZE_KB = 140
MAX_WIDTH = 1200

for image_path in source_dir.glob("*.png"):
    with Image.open(image_path) as original:
        image = original.convert("RGB")

        if image.width > MAX_WIDTH:
            new_height = int(image.height * MAX_WIDTH / image.width)
            image = image.resize(
                (MAX_WIDTH, new_height),
                Image.Resampling.LANCZOS
            )

        quality = 85

        while True:
            buffer = BytesIO()
            image.save(
                buffer,
                format="JPEG",
                quality=quality,
                optimize=True
            )

            if buffer.tell() <= MAX_SIZE_KB * 1024:
                break

            if quality > 30:
                quality -= 5
            else:
                new_width = int(image.width * 0.85)
                new_height = int(image.height * 0.85)

                image = image.resize(
                    (new_width, new_height),
                    Image.Resampling.LANCZOS
                )
                quality = 85

        output_path = output_dir / (image_path.stem + ".jpg")
        output_path.write_bytes(buffer.getvalue())

        print(
            f"{image_path.name} -> "
            f"{output_path.name}: "
            f"{output_path.stat().st_size / 1024:.1f} KB"
        )
