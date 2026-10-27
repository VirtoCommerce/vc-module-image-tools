// The server's own message instead of the generated client's generic one
export function errorText(e: unknown) {
  const err = e as { response?: string; message?: string };
  try {
    const body = JSON.parse(err.response ?? "");
    if (body?.message || body?.detail || body?.title) return body.message ?? body.detail ?? body.title;
  } catch {
    // not JSON
  }
  return err.message ?? String(e);
}
