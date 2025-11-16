#!/usr/bin/env python3
"""
Utility para regenerar el diagrama de estados de MetaAhorro en PDF sin depender
de herramientas externas. Dibuja primitivas básicas (texto, rectas y curvas)
directamente en un canvas PDF de tamaño carta.
"""
from __future__ import annotations

import math
from dataclasses import dataclass
from io import BytesIO
from pathlib import Path
from typing import Iterable, List, Sequence, Tuple, Union


Point = Tuple[float, float]
Label = Union[str, Sequence[str]]


def _fmt(value: float) -> str:
    if abs(value) < 1e-4:
        return "0"
    text = f"{value:.3f}".rstrip("0").rstrip(".")
    return text or "0"


def _escape(text: str) -> str:
    return text.replace("\\", "\\\\").replace("(", "\\(").replace(")", "\\)")


class PdfCanvas:
    def __init__(self, width: int = 612, height: int = 792) -> None:
        self.width = width
        self.height = height
        self.commands: List[str] = [
            "1 w",  # stroke width
            "0 0 0 RG",  # stroke color (black)
            "0 0 0 rg",  # fill color (black)
        ]

    def _emit(self, command: str) -> None:
        self.commands.append(command)

    def text(self, x: float, y: float, message: str, size: float = 10) -> None:
        self._emit(
            f"BT /F1 {_fmt(size)} Tf 1 0 0 1 {_fmt(x)} {_fmt(y)} Tm "
            f"({_escape(message)}) Tj ET"
        )

    def multiline_text(
        self, x: float, y: float, lines: Iterable[str], size: float = 9, leading: float = 11
    ) -> None:
        for index, line in enumerate(lines):
            self.text(x, y - index * leading, line, size)

    def circle(self, cx: float, cy: float, r: float, fill: bool = False) -> None:
        k = 0.5522847498  # aproximación de Bezier
        commands = [
            f"{_fmt(cx + r)} {_fmt(cy)} m",
            f"{_fmt(cx + r)} {_fmt(cy + k * r)} {_fmt(cx + k * r)} {_fmt(cy + r)} {_fmt(cx)} {_fmt(cy + r)} c",
            f"{_fmt(cx - k * r)} {_fmt(cy + r)} {_fmt(cx - r)} {_fmt(cy + k * r)} {_fmt(cx - r)} {_fmt(cy)} c",
            f"{_fmt(cx - r)} {_fmt(cy - k * r)} {_fmt(cx - k * r)} {_fmt(cy - r)} {_fmt(cx)} {_fmt(cy - r)} c",
            f"{_fmt(cx + k * r)} {_fmt(cy - r)} {_fmt(cx + r)} {_fmt(cy - k * r)} {_fmt(cx + r)} {_fmt(cy)} c",
            "h",
            "B" if fill else "S",
        ]
        self._emit("\n".join(commands))

    def rectangle(self, x: float, y: float, w: float, h: float) -> None:
        self._emit(f"{_fmt(x)} {_fmt(y)} {_fmt(w)} {_fmt(h)} re S")

    def state_box(
        self, x: float, y: float, w: float, h: float, title: str, details: Sequence[str]
    ) -> None:
        self.rectangle(x, y, w, h)
        self.text(x + 10, y + h - 18, title, 12)
        self.multiline_text(x + 10, y + h - 34, details, 9)

    def arrow_path(
        self,
        points: Sequence[Point],
        label: Label | None = None,
        label_offset: Point = (0, 0),
        font_size: float = 9,
    ) -> None:
        if len(points) < 2:
            raise ValueError("Se requieren al menos dos puntos para una flecha.")
        commands = [f"{_fmt(points[0][0])} {_fmt(points[0][1])} m"]
        for point in points[1:]:
            commands.append(f"{_fmt(point[0])} {_fmt(point[1])} l")
        commands.append("S")
        self._emit("\n".join(commands))

        x2, y2 = points[-1]
        x1, y1 = points[-2]
        angle = math.atan2(y2 - y1, x2 - x1)
        arrow_len = 10
        left = angle + math.pi - math.pi / 9
        right = angle + math.pi + math.pi / 9
        left_point = (x2 + arrow_len * math.cos(left), y2 + arrow_len * math.sin(left))
        right_point = (x2 + arrow_len * math.cos(right), y2 + arrow_len * math.sin(right))
        self._emit(
            f"{_fmt(x2)} {_fmt(y2)} m {_fmt(left_point[0])} {_fmt(left_point[1])} l S"
        )
        self._emit(
            f"{_fmt(x2)} {_fmt(y2)} m {_fmt(right_point[0])} {_fmt(right_point[1])} l S"
        )

        if label:
            avg_x = sum(p[0] for p in points) / len(points) + label_offset[0]
            avg_y = sum(p[1] for p in points) / len(points) + label_offset[1]
            lines = label if isinstance(label, Sequence) and not isinstance(label, (str, bytes)) else [label]  # type: ignore[arg-type]
            self.multiline_text(avg_x, avg_y, lines, size=font_size)

    def save(self, path: Path) -> None:
        content = "\n".join(self.commands).encode("latin-1")
        buffer = BytesIO()
        buffer.write(b"%PDF-1.4\n")
        offsets = {}

        def write_obj(num: int, body: bytes) -> None:
            offsets[num] = buffer.tell()
            buffer.write(f"{num} 0 obj\n".encode("ascii"))
            buffer.write(body)
            buffer.write(b"\nendobj\n")

        write_obj(1, b"<< /Type /Catalog /Pages 2 0 R >>")
        write_obj(2, b"<< /Type /Pages /Kids [3 0 R] /Count 1 >>")
        write_obj(
            3,
            b"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] "
            b"/Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
        )
        write_obj(4, b"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>")
        write_obj(
            5,
            b"<< /Length " + str(len(content)).encode("ascii") + b" >>\nstream\n" + content + b"endstream",
        )
        xref_pos = buffer.tell()
        buffer.write(b"xref\n0 6\n")
        buffer.write(b"0000000000 65535 f \n")
        for i in range(1, 6):
            buffer.write(f"{offsets[i]:010d} 00000 n \n".encode("ascii"))
        buffer.write(b"trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n")
        buffer.write(f"{xref_pos}\n".encode("ascii"))
        buffer.write(b"%%EOF")
        path.write_bytes(buffer.getvalue())


def build_diagram() -> None:
    pdf = PdfCanvas()

    # Encabezado
    pdf.text(72, 755, "Maquina de estados: MetaAhorro", 16)
    pdf.multiline_text(
        72,
        735,
        [
            "Entidad: MetaAhorro (Spendnt.Shared/Entities/MetaAhorro)",
            "Atributo de control: bool EstaCompletada",
            "Eventos: CrearMeta, RegistrarTransaccion, ContribuirMeta, ActualizarMeta, EliminarMeta",
            "Guardas basadas en MontoActual, MontoObjetivo y banderas enviadas por el cliente",
        ],
        size=10,
        leading=12,
    )

    # Estados
    pdf.circle(95, 600, 12, fill=True)
    pdf.text(60, 570, "Estado inicial", 9)

    pdf.state_box(
        x=200,
        y=470,
        w=200,
        h=130,
        title="En Progreso",
        details=[
            "EstaCompletada = false",
            "MontoActual < MontoObjetivo",
        ],
    )
    pdf.state_box(
        x=400,
        y=300,
        w=200,
        h=130,
        title="Completada",
        details=[
            "EstaCompletada = true",
            "MontoActual >= MontoObjetivo",
        ],
    )

    pdf.circle(520, 170, 18)
    pdf.circle(520, 170, 12)
    pdf.text(480, 138, "Estado final: Eliminada", 10)

    # Transiciones desde el inicio
    pdf.arrow_path(
        [(107, 600), (200, 600)],
        label=["CrearMeta()", "[ModelState válido]"],
        label_offset=(-15, 18),
    )
    pdf.arrow_path(
        [(107, 600), (180, 690), (520, 690), (520, 430)],
        label=["CrearMeta()", "[EstaCompletada = true]", "/ inicializarMeta()"],
        label_offset=(-10, 18),
    )

    # Autotransiciones
    pdf.arrow_path(
        [(250, 520), (150, 520), (150, 420), (250, 420), (250, 520)],
        label=[
            "Registrar/Contribuir",
            "[MontoActual + aporte < objetivo]",
            "/ ActualizarMontos()",
        ],
        label_offset=(-20, -10),
    )
    pdf.arrow_path(
        [(540, 340), (640, 340), (640, 430), (540, 430), (540, 340)],
        label=[
            "Registrar/Contribuir",
            "[Se mantiene >= objetivo]",
            "/ MantenerSaldo()",
        ],
        label_offset=(15, 10),
    )

    # Transiciones entre estados
    pdf.arrow_path(
        [(360, 520), (450, 430)],
        label=[
            "Registrar/Contribuir",
            "[MontoActual + aporte >= objetivo]",
            "/ ActualizarMontos()",
        ],
        label_offset=(-10, -15),
    )
    pdf.arrow_path(
        [(450, 360), (360, 450)],
        label=[
            "ActualizarMeta() o aporte negativo",
            "[EstaCompletada = false]",
            "/ PersistirCambios()",
        ],
        label_offset=(-15, 10),
    )

    # Eliminación
    pdf.arrow_path(
        [(280, 470), (360, 360), (480, 240), (520, 190)],
        label=["EliminarMeta()", "/ BorrarRegistro()"],
        label_offset=(-15, -20),
    )
    pdf.arrow_path(
        [(520, 300), (520, 190)],
        label=["EliminarMeta()", "/ BorrarRegistro()"],
        label_offset=(20, -40),
    )

    # Notas
    pdf.multiline_text(
        72,
        260,
        [
            "Notas:",
            "- Contribuciones negativas se permiten solo vía actualización manual.",
            "- Eliminada representa la baja física del registro (estado terminal).",
        ],
        size=8.5,
    )

    output_path = Path(__file__).with_name("meta-ahorro-state-machine.pdf")
    pdf.save(output_path)
    print(f"PDF actualizado en {output_path}")


if __name__ == "__main__":
    build_diagram()
