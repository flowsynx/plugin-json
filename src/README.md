## FlowSynx JSON Plugin

The **FlowSynx JSON Plugin** is a built-in, plug-and-play integration for the FlowSynx automation engine. It enables parsing, transforming, extracting, and mapping structured or semi-structured JSON data within workflows with no custom coding required.

This plugin is automatically installed by the FlowSynx engine when selected in the workflow builder. It is not intended for standalone developer usage outside the FlowSynx platform.

---

## Purpose

The JSON Plugin allows FlowSynx users to:

- Parse and inspect complex JSON structures.
- Extract values using JSONPath.
- Flatten nested objects.
- Map JSON data to specific output fields using flexible rules.

It integrates seamlessly into FlowSynx no-code/low-code workflows.

---

## Supported Operations

- **extract**: Extracts a specific value using a `jsonPath` expression.
- **transform**: Flattens a nested JSON structure into flat `key: value` pairs.
- **map**: Maps fields to new keys using a dictionary of JSONPath expressions.

---

## Input Parameters

Below are the parameters accepted by the plugin:

- `Operation` (string): Required. The type of operation to perform. Supported values are `extract`, `transform`, and `map`.
- `Json` (string): Required. The raw JSON string to process.
- `jsonPath` (string): Required for `extract` operation. A JSONPath expression to locate a specific value.
- `Mappings` (dictionary): Required for `map` operation. A dictionary where each key is an output field and each value is a JSONPath expression.
- `Flatten` (bool): Optional. Used with `transform` to specify whether to flatten nested objects (`true`) or not (`false`).

Example input:
```json
{
  "Operation": "map",
  "Json": "{...}",
  "jsonPath": "$.some.path",
  "Mappings": {
    "Name": "$.person.name",
    "Email": "$.person.contact.email"
  },
  "Flatten": true
}
```

---

## Operation Examples

### extract Operation

**Input JSON:**
```json
{
  "meta": {
    "version": "1.0.2",
    "timestamp": "2024-12-01T10:00:00Z"
  }
}
```

**Input Parameters:**
```json
{
  "Operation": "extract",
  "Json": "{...}",
  "jsonPath": "$.meta.version"
}
```

**Output:**
```json
"1.0.2"
```

---

### transform Operation

**Input JSON:**
```json
{
  "user": {
    "profile": {
      "name": "Bob",
      "email": "bob@example.com"
    }
  }
}
```

**Input Parameters:**
```json
{
  "Operation": "transform",
  "Json": "{...}",
  "Flatten": true
}
```

**Output:**
```json
{
  "user.profile.name": "Bob",
  "user.profile.email": "bob@example.com"
}
```

---

### map Operation

**Input JSON:**
```json
{
  "person": {
    "name": "Alice",
    "contact": {
      "email": "alice@example.com"
    }
  }
}
```

**Input Parameters:**
```json
{
  "Operation": "map",
  "Json": "{...}",
  "Mappings": {
    "Name": "$.person.name",
    "Email": "$.person.contact.email"
  }
}
```

**Output:**
```json
{
  "Name": "Alice",
  "Email": "alice@example.com"
}
```

---

## Example Use Case in FlowSynx

1. Add the JSON plugin to your FlowSynx workflow.
2. Set `Operation` to one of: `extract`, `transform`, or `map`.
3. Provide the JSON input string.
4. Configure `jsonPath`, `Mappings`, or `Flatten` depending on the operation.
5. Use the output downstream in your workflow.

---

## Debugging Tips

- If the result is null, ensure `jsonPath` is valid and points to an existing element.
- If nothing is mapped, check for typos in the JSONPath expressions inside `Mappings`.
- If the result isn't flattened, double-check that `Flatten` is set to `true`.

---

## Security Notes

- No data is persisted unless explicitly configured.
- All operations run in a secure sandbox within FlowSynx.
- Only authorized platform users can view or modify configurations.

---

## License

Copyright FlowSynx. All rights reserved.