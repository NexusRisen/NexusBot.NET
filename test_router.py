import urllib.request, json
urls = [
    'https://router.huggingface.co/hf-inference/models/Qwen/Qwen2.5-7B-Instruct/v1/chat/completions',
    'https://router.huggingface.co/models/Qwen/Qwen2.5-7B-Instruct/v1/chat/completions',
    'https://router.huggingface.co/together/models/Qwen/Qwen2.5-7B-Instruct/v1/chat/completions'
]
for u in urls:
    try:
        req = urllib.request.Request(u, data=b'{"model":"Qwen/Qwen2.5-7B-Instruct","messages":[{"role":"user","content":"test"}]}', headers={'Content-Type': 'application/json'})
        urllib.request.urlopen(req)
        print(f'{u} -> OK')
    except Exception as e:
        print(f'{u} -> {getattr(e, "code", getattr(e, "reason", "error"))}')
