# daytonaraceway.cli
CLI tool to parse results &amp; create protocols of events held at Daytona Raceway [Cyprus].

## CLI commands

### Qualification results

#### By stage
```shell
daytona qualification <RACE_UUID> -s <STAGE_ID>
```

| Option                  | Description                                                                                     | Required | Default      | Possible Values            |
|-------------------------|-------------------------------------------------------------------------------------------------|----------|--------------|----------------------------|
| -s, --stage <STAGE_ID>  | Stage ID (can be found in 'Session management' tab right next to `<RACE_UUID>`)                 | +        | -            | any stage number           |
| -f, --format <FORMAT>   | Output format                                                                                   | -        | Console      | Console, Html, Csv         |
| --sourceip <IP>         | IP address of the API host                                                                      | -        | 213.7.195.58 | any valid IP               |

#### Total results from multiple stages
```shell
daytona qualification <RACE_UUID> -o <BestLap|AggLap|Participant>
```

| Option                  | Description                                          | Required | Default      | Possible Values            |
|-------------------------|------------------------------------------------------|----------|--------------|----------------------------|
| -o, --order <ORDER>     | Ordering of aggregated results across all stages     | -        | BestLap      | BestLap, AggLap, Participant |
| -f, --format <FORMAT>   | Output format                                        | -        | Console      | Console, Html, Csv         |
| --sourceip <IP>         | IP address of the API host                           | -        | 213.7.195.58 | any valid IP               |

`<RACE_UUID>` — uuid of a race event (can be found either in the admin URL or in the 'Session management' tab).

* `qualy` can be used as a short alias instead of `qualification`.

### Groups distribution after qualification
```shell
daytona groups <RACE_UUID> -c <N> -o <BestLap|AggLap> -m <Block|Chequered|Random> -f <Console|Html|Csv>
```
| Option                  | Description                                                   | Required | Default      | Possible Values          |
|-------------------------|---------------------------------------------------------------|----------|--------------|--------------------------|
| -c, --count <N>         | Number of groups to split participants to                     | +        | -            | [1, 702]                 |
| -m, --mode <MODE>       | Distribution mode of splitting into groups                    | -        | Chequered    | Block, Chequered, Random |
| -o, --order <ORDER>     | Sorting of qualification results before splitting into groups | -        | BestLap      | BestLap, AggLap          |
| -f, --format <FORMAT>   | Output format                                                 | -        | Console      | Console, Html, Csv       |
| --sourceip <IP>         | IP address of the API host                                    | -        | 213.7.195.58 | any valid IP             |

### Race results

#### By stage
```shell
daytona race <RACE_UUID> -s <STAGE_ID>
```

#### Total results
```shell
daytona race <RACE_UUID>
```

> Total results are always output as Html regardless of the `-f` option.

| Option                  | Description                                                              | Required | Default      | Possible Values    |
|-------------------------|--------------------------------------------------------------------------|----------|--------------|--------------------|
| -s, --stage <STAGE_ID>  | Stage ID. If omitted, results are aggregated across all stages.          | -        | -            | any stage number   |
| --finals-order <ORDER>  | Ordering of heats in the final stage for points distribution             | -        | Asc          | Asc, Desc          |
| -f, --format <FORMAT>   | Output format (single-stage only)                                        | -        | Console      | Console, Html, Csv |
| --sourceip <IP>         | IP address of the API host                                               | -        | 213.7.195.58 | any valid IP       |

### Points scale
```shell
daytona points -c <N> -t <Stage|Final|Championship>
```

| Option              | Description                                                                      | Required | Default | Possible Values              |
|---------------------|----------------------------------------------------------------------------------|----------|---------|------------------------------|
| -c, --count <N>     | Number of participants for which the scale is calculated. Must be at least 3.    | +        | -       | >= 3 (>= 6 for Championship) |
| -t, --type <TYPE>   | Points scale type                                                                | +        | -       | Stage, Final, Championship   |
