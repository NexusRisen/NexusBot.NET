import urllib.request
req = urllib.request.Request(
    'https://router.huggingface.co/hf-inference/models/mistralai/Mistral-7B-Instruct-v0.2/v1/chat/completions',
    data=b'{}',
    headers={'Content-Type': 'application/json'}
)
try:
    print(urllib.request.urlopen(req).read())
except Exception as e:
    print(e.read() if hasattr(e, 'read') else e)
