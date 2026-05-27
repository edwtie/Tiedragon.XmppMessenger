<?php
declare(strict_types=1);

const MAX_UPLOAD_BYTES = 10_485_760;

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');

try {
    if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
        http_response_code(405);
        echo json_encode(['ok' => false, 'error' => 'method_not_allowed']);
        return;
    }

    if (!isset($_FILES['file']) || !is_array($_FILES['file'])) {
        http_response_code(400);
        echo json_encode(['ok' => false, 'error' => 'missing_file']);
        return;
    }

    $file = $_FILES['file'];
    if (($file['error'] ?? UPLOAD_ERR_NO_FILE) !== UPLOAD_ERR_OK) {
        http_response_code(400);
        echo json_encode(['ok' => false, 'error' => 'upload_failed', 'code' => $file['error'] ?? null]);
        return;
    }

    $size = (int)($file['size'] ?? 0);
    if ($size <= 0 || $size > MAX_UPLOAD_BYTES) {
        http_response_code(413);
        echo json_encode(['ok' => false, 'error' => 'file_too_large', 'maxBytes' => MAX_UPLOAD_BYTES]);
        return;
    }

    $originalName = sanitizeFileName((string)($file['name'] ?? 'upload.bin'));
    $extension = strtolower(pathinfo($originalName, PATHINFO_EXTENSION));
    $storedName = bin2hex(random_bytes(12)) . ($extension !== '' ? '.' . $extension : '');
    $uploadDir = dirname(__DIR__) . DIRECTORY_SEPARATOR . 'uploads';

    if (!is_dir($uploadDir) && !mkdir($uploadDir, 0775, true) && !is_dir($uploadDir)) {
        throw new RuntimeException('Cannot create upload directory.');
    }

    $target = $uploadDir . DIRECTORY_SEPARATOR . $storedName;
    if (!move_uploaded_file((string)$file['tmp_name'], $target)) {
        throw new RuntimeException('Cannot store uploaded file.');
    }

    $mime = detectMime($target, (string)($file['type'] ?? ''));
    echo json_encode([
        'ok' => true,
        'file' => [
            'name' => $originalName,
            'storedName' => $storedName,
            'url' => 'uploads/' . rawurlencode($storedName),
            'size' => $size,
            'type' => $mime,
            'uploadedAt' => gmdate('c'),
        ],
    ], JSON_UNESCAPED_SLASHES);
} catch (Throwable $error) {
    http_response_code(500);
    echo json_encode(['ok' => false, 'error' => 'server_error', 'message' => $error->getMessage()]);
}

function sanitizeFileName(string $name): string
{
    $base = basename(str_replace('\\', '/', $name));
    $base = preg_replace('/[^A-Za-z0-9._ -]/', '_', $base) ?: 'upload.bin';
    $base = trim($base, " .\t\n\r\0\x0B");
    return $base !== '' ? substr($base, 0, 180) : 'upload.bin';
}

function detectMime(string $path, string $fallback): string
{
    if (function_exists('finfo_open')) {
        $info = finfo_open(FILEINFO_MIME_TYPE);
        if ($info !== false) {
            $mime = finfo_file($info, $path);
            if (is_string($mime) && $mime !== '') {
                return $mime;
            }
        }
    }

    return $fallback !== '' ? $fallback : 'application/octet-stream';
}
