# daytonaraceway.cli
CLI tool to parse results &amp; create protocols of events held at Daytona Raceway [Cyprus].

## CLI arguments

`<RACE_UUID>` — uuid of a race event (can be found either in the admin URL or in the 'Session management' tab).

## CLI commands

### Qualification results

#### By stage
```shell
daytona qualification <RACE_UUID> -s <STAGE_ID> -f <html|csv>
```

#### Total results from multiple stages
```shell
daytona qualification <RACE_UUID> -o <BestLap|AggLap> -f <html|csv>
```

| Option                 | Description                                                                     | Required | Default | Possible Values    |
|------------------------|---------------------------------------------------------------------------------|----------|---------|--------------------|
| -s, --stage <STAGE_ID> | Stage ID (can be found in 'Session management' tab right next to `<RACE_UUID>`) | +        | -       | any stage number   |
| -o, --order <ORDER>    | Ordering of aggregated results across all stages                                | -        | BestLap | BestLap, AggLap    |
| -f, --format <FORMAT>  | Output format                                                                   | -        | Console | Console, Html, Csv |

> `qualy` can be used as a short alias instead of `qualification`.

### Groups distribution after qualification
```shell
daytona groups <RACE_UUID> -c <N> -o <BestLap|AggLap> -m <block|chequered|random> -f <html|csv>
```

| Option                  | Description                                                   | Required | Default   | Possible Values          |
|-------------------------|---------------------------------------------------------------|----------|-----------|--------------------------|
| -c, --count <N>         | Number of groups to split participants to                     | +        | -         | [1, 702]                 |
| -o, --order <ORDER>     | Sorting of qualification results before splitting into groups | -        | BestLap   | BestLap, AggLap          |
| -m, --mode <MODE>       | Distribution mode of splitting into groups                    | -        | Chequered | Block, Chequered, Random |
| -f, --format <FORMAT>   | Output format                                                 | -        | Console   | Console, Html, Csv       |


### Race results

#### By stage
```shell
daytona race <RACE_UUID> -s <STAGE_ID> -f <html|csv>
```

#### Total results
```shell
daytona race <RACE_UUID> -f <html|csv> --finals-order <asc|desc>
```

> Total results are always output as Html regardless of the `-f` option.

| Option                  | Description                                                    | Required | Default      | Possible Values    |
|-------------------------|----------------------------------------------------------------|----------|--------------|--------------------|
| -s, --stage <STAGE_ID>  | Stage ID. If omitted, results are aggregated across all stages | -        | -            | any stage number   |
| --finals-order <ORDER>  | Ordering of heats in the final stage for points distribution   | -        | Asc          | Asc, Desc          |
| -f, --format <FORMAT>   | Output format (single-stage only)                              | -        | Console      | Console, Html, Csv |

### Points scale
```shell
daytona points -c <N> -t <stage|final|championship>
```

| Option              | Description                                                                      | Required | Default | Possible Values              |
|---------------------|----------------------------------------------------------------------------------|----------|---------|------------------------------|
| -c, --count <N>     | Number of participants for which the scale is calculated. Must be at least 3.    | +        | -       | >= 3 (>= 6 for Championship) |
| -t, --type <TYPE>   | Points scale type                                                                | +        | -       | Stage, Final, Championship   |
