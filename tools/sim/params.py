"""All tunable numbers for the lot simulation, in one place.

Every value marked REPO is read from the Unity project (path in comment). Every value marked
ASSUMPTION is a player-behaviour guess that the game does not define. Units: Unity world units,
real seconds ("s") for human actions, game minutes ("min") for game rules. 1 game minute = 1/1.5 s.
"""

# --- Time (REPO: Data/Economy/RunConfig.asset, Economy/RunConfig.cs) ---------------------------
GAME_MIN_PER_SEC = 1.5
NIGHT_MIN = 180
RUN_NIGHTS = 7
STARTING_DEBT = 1500

# --- Patrons (REPO: Data/Patrons/PatronConfig.asset) -------------------------------------------
PATRON = dict(
    gap=(8.0, 15.0), stay=(20.0, 45.0), base_tip=20, impatient=0.25,
    park_window=45.0, pickup_window=60.0,
    ev_start=0.2, charge_min=20.0, wash_min=15.0,  # PatronConfig.cs defaults (asset omits them)
)

# --- Tips (REPO: Data/Economy/TipConfig.asset, Economy/TipConfig.cs) ---------------------------
TIP = dict(window=10.0, bonus=2, impatient_bonus=6, penalty_per_min=0.5,
           charge_bonus=15, clean_bonus=15)

# --- Lot geometry (REPO: Data/Lot/LotConfig.asset, Lot/LotGeometry.cs) -------------------------
LOT = dict(
    rows=2, spot=(2.6, 4.4), spot_gap=0.2, aisle=5.0,
    curb_pos=(-12.0, 0.0), curb_size=(3.0, 6.0), curb_to_spots=3.0,
    queue_car=(2.2, 4.5), queue_gap=0.6, queue_start_gap=0.8, queue_slots=3,
    charging_bays=2, station=(2.6, 4.4), station_gap=0.4, stations_first=(-17.5, 6.0),
    valet_start_offset=1.5,
)

# --- Movement (REPO: Valet/OnFootMover.cs + Bootstrap scene _speed 4, Interactor _range 2,
#     SeatController _exitDistance 1.5, CarDefinition Sedan.asset, LotConfig settle) ------------
MOVE = dict(walk=4.0, interact_range=2.0, car_top=8.0, car_accel=6.0, car_brake=12.0,
            settle_s=0.5)

# --- Human action overheads (ASSUMPTION: the human's example values) ---------------------------
OVERHEAD_S = dict(enter=1.5, exit=1.0, park_precision=3.0, curb_precision=2.0, station_precision=3.0)

# --- Night plans (REPO: Night/NightPlanDefaults.cs): cap, gap scale, EV, dirty, charging, wash --
DEFAULT_PLANS = [
    # night, max_waiting, gap_scale, ev_chance, dirty_chance, charging_open, wash_open
    (1, 1, 1.0, 0.0, 0.0, False, False),
    (2, 3, 0.7, 0.0, 0.0, False, False),
    (3, 3, 0.7, 0.4, 0.0, True, False),
    (4, 3, 0.7, 0.0, 0.4, False, True),
    (5, 3, 0.6, 0.3, 0.3, True, True),
    (6, 3, 0.6, 0.3, 0.3, True, True),
    (7, 3, 0.6, 0.3, 0.3, True, True),
]
CURRENT_SPOTS = 12  # LotConfig: 2 rows x 6 per row

# --- Monte Carlo ---------------------------------------------------------------------------------
SEED = 20261106
RUNS = 500
SPOT_RANGE = range(4, 25)
