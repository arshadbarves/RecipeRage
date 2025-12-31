# External Code References

## LLM & Code Execution Logic
The following snippets were provided by the user for reference regarding LLM request handling and code execution. This logic is likely part of an external backend or tool integration.

### LLM Class Configuration
```python
class LLM:
        VOCAB_SIZE = "{arch}.vocab_size"
        CONTEXT_LENGTH = "{arch}.context_length"
        # ... (Architectural constants)
```

### LLM Engine Wrapper
```python
class LLM:
    """An LLM for generating texts from given prompts and sampling parameters.
    ...
    """
    def __init__(self, model, tokenizer, ...):
        # ...
    
    def generate(self, prompts, ...):
        # ...
```

### LLM Client / Request Handler
```python
class LLM:
    _instances: Dict[str, "LLM"] = {}
    
    def __init__(self, config_name="default", ...):
        # ... (Initializes AsyncOpenAI or Azure client)

    async def ask(self, messages, ...):
        # ... (Handles chat completions with token counting and retries)

    async def ask_with_images(self, messages, images, ...):
        # ... (Handles multimodal requests)
```

### Code Execution
```python
def execute_code(
      self,
      invocation_context: InvocationContext,
      code_execution_input: CodeExecutionInput,
  ) -> CodeExecutionResult:
    # ... (Executes python code, captures stdout/stderr)
```
