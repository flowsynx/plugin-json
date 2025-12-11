# FlowSynx JSON Plugin

The **FlowSynx JSON Plugin** is a built-in, plug-and-play integration for the FlowSynx automation engine. It enables parsing, transforming, extracting, and mapping structured or semi-structured JSON data within workflows with no custom coding required.

This plugin is automatically installed by the FlowSynx engine when selected in the workflow builder. It is not intended for standalone developer usage outside the FlowSynx platform.

## Purpose

The JSON Plugin allows FlowSynx users to:

- Parse and inspect complex JSON structures.
- Extract values using JSONPath-like expressions.
- Flatten nested objects.
- Map JSON data to specific output fields using flexible rules.
- Format the output with indentation for readability.

It integrates seamlessly into FlowSynx no-code/low-code workflows.

## Supported Operations

- **extract**: Extracts a specific value using a `Path` expression.
- **transform**: Flattens a nested JSON structure into flat `key: value` pairs.
- **map**: Maps fields to new keys using a dictionary of `Path` expressions.

Operation names are case-insensitive and passed as part of the `InputParameter`.

## Input Parameters

The plugin accepts the following parameters:

- `Operation` (string): Required. The type of operation to perform. Supported values are `extract`, `transform`, and `map`.
- `Data` (object): Required. The JSON object (or raw JSON string) to process.
- `Path` (string): Required for `extract` operation. A JSONPath-like expression to locate a specific value.
- `Mappings` (dictionary): Required for `map` operation. A dictionary where each key is an output field and each value is a `Path` expression.
- `Flatten` (bool): Optional. Used with `transform` to specify whether to flatten nested objects (`true`) or not (`false`).
- `Indented` (bool): Optional. Determines whether the output JSON should be pretty-printed with indentation (`true`) or compact (`false`).

### Map operation example input

```json
{
  "Data": { ... },
  "Path": "$.some.path",
  "Mappings": {
    "Name": "$.person.name",
    "Email": "$.person.contact.email"
  },
  "Flatten": true,
  "Indented": true
}
```

## Operation Examples

### extract Operation

**Input JSON Object:**
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
  "Data": { ... },
  "Path": "$.meta.version"
}
```

**Output:**
```json
"1.0.2"
```

---

**Input JSON Array:**
```json
[
  { "id": 1 },
  { "id": 2 },
  { "id": 3 }
]
```

**Input Parameters:**
```json
{
  "Data": "[...]",
  "Path": "$[*].id"
}
```

**Output:**
```json
[
    1,
    2,
    3
]
```

### transform Operation

**Input Data:**
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
  "Data": { ... },
  "Flatten": true,
  "Indented": true
}
```

**Output:**
```json
{
  "user.profile.name": "Bob",
  "user.profile.email": "bob@example.com"
}
```

### map Operation

**Input Data:**
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
  "Data": { ... },
  "Mappings": {
    "Name": "$.person.name",
    "Email": "$.person.contact.email"
  },
  "Indented": false
}
```

**Output:**
```json
{
  "Name": "Alice",
  "Email": "alice@example.com"
}
```

## Example Use Case in FlowSynx

1. Add the JSON plugin to your FlowSynx workflow.
3. Provide the JSON input object in `Data`.
4. Configure `Path`, `Mappings`, `Flatten`, and `Indented` depending on the operation.
5. Use the plugin output downstream in your workflow.

## Debugging Tips

- If the result is null, ensure `Path` is valid and points to an existing element.
- If nothing is mapped, check for typos in the `Path` expressions inside `Mappings`.
- To get human-readable output, set `Indented` to `true`.

## Security Notes

- No data is persisted unless explicitly configured.
- All operations run in a secure sandbox within FlowSynx.
- Only authorized platform users can view or modify configurations.

## License

Copyright FlowSynx. All rights reserved.