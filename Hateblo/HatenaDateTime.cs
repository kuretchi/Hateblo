using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Hateblo.Internal;

namespace Hateblo
{
    /// <summary>
    /// 世界協定時刻 (UTC) を基準とする相対的な日時を表します。はてなのサービスで使用されている形式と互換性があります。
    /// </summary>
    public struct HatenaDateTime
    {
        private static readonly string _pattern
            = @"^\s*"
            + @"(?<year>\d{4})"
            + "-"
            + @"(?<month>\d{2})"
            + "-"
            + @"(?<day>\d{2})"
            + "T"
            + @"(?<hour>\d{2})"
            + ":"
            + @"(?<minute>\d{2})"
            + ":"
            + @"(?<second>\d{2})"
            + @"((\+(?!-))?(?<offset>-?\d{2}:\d{2}(:\d{2})?)|(?<offset>Z))"
            + @"\s*$";

        private static readonly Regex _regex
            = new Regex(_pattern);

        private readonly int _month, _day;

        /// <summary>
        /// 指定された <see cref="DateTimeOffset"/> 値で、 <see cref="HatenaDateTime"/> 構造体の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dateTimeOffset">世界協定時刻 (UTC) を基準とする相対的な日時。</param>
        public HatenaDateTime(DateTimeOffset dateTimeOffset)
        {
            this.Year = dateTimeOffset.Year;
            _month = dateTimeOffset.Month - 1;
            _day = dateTimeOffset.Day - 1;
            this.Hour = dateTimeOffset.Hour;
            this.Minute = dateTimeOffset.Minute;
            this.Second = dateTimeOffset.Second;
            this.Offset = dateTimeOffset.Offset;
        }

        /// <summary>
        /// 指定された年月日時分秒で、 <see cref="HatenaDateTime"/> 構造体の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="year">年 (0 から 9999)。</param>
        /// <param name="month">月 (1 から 12)。</param>
        /// <param name="day">日 (1 から <paramref name="month"/> の日数)。</param>
        /// <param name="hour">時 (0 から 23)。</param>
        /// <param name="minute">分 (0 から 59)。</param>
        /// <param name="second">秒 (0 から 59)。</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="year"/>、 <paramref name="month"/>、 <paramref name="day"/>、 <paramref name="hour"/>、 <paramref name="minute"/>、 <paramref name="second"/> のいずれかが、有効な範囲にありません。</exception>
        public HatenaDateTime(int year, int month, int day, int hour, int minute, int second)
            : this(year, month, day, hour, minute, second, TimeSpan.Zero) { }

        /// <summary>
        /// 指定された年月日時分秒、およびオフセットで、 <see cref="HatenaDateTime"/> 構造体の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="year">年 (0 から 9999)。</param>
        /// <param name="month">月 (1 から 12)。</param>
        /// <param name="day">日 (1 から <paramref name="month"/> の日数)。</param>
        /// <param name="hour">時 (0 から 23)。</param>
        /// <param name="minute">分 (0 から 59)。</param>
        /// <param name="second">秒 (0 から 59)。</param>
        /// <param name="offset">世界協定時刻 (UTC) からの時刻のオフセット (-14 時間から 14 時間)。</param>
        /// <exception cref="ArgumentOutOfRangeException">><paramref name="year"/>、 <paramref name="month"/>、 <paramref name="day"/>、 <paramref name="hour"/>、 <paramref name="minute"/>、 <paramref name="second"/>、および <paramref name="offset"/> のいずれかが、有効な範囲にありません。</exception>
        public HatenaDateTime(int year, int month, int day, int hour, int minute, int second, TimeSpan offset)
        {
            Validation.InRange(year, 0, 9999, nameof(year));
            Validation.InRange(month, 1, 12, nameof(month));
            Validation.InRange(day, 1, DateTime.DaysInMonth(year == 0 ? 4 : year, month), nameof(day), maxValueName: "days in month");
            Validation.InRange(hour, 0, 23, nameof(hour));
            Validation.InRange(minute, 0, 59, nameof(minute));
            Validation.InRange(second, 0, 59, nameof(second));
            Validation.InRange(offset, TimeSpan.FromHours(-14), TimeSpan.FromHours(14), nameof(offset), "-14 hours", "14 hours");

            this.Year = year;
            _month = month - 1;
            _day = day - 1;
            this.Hour = hour;
            this.Minute = minute;
            this.Second = second;
            this.Offset = offset;
        }

        /// <summary>
        /// 年を取得します。
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// 月を取得します。
        /// </summary>
        public int Month => _month + 1;

        /// <summary>
        /// 日を取得します。
        /// </summary>
        public int Day => _day + 1;

        /// <summary>
        /// 時を取得します。
        /// </summary>
        public int Hour { get; }

        /// <summary>
        /// 分を取得します。
        /// </summary>
        public int Minute { get; }

        /// <summary>
        /// 秒を取得します。
        /// </summary>
        public int Second { get; }

        /// <summary>
        /// 世界協定時刻 (UTC) からの時刻のオフセットを取得します。
        /// </summary>
        public TimeSpan Offset { get; }

        /// <summary>
        /// 現在の <see cref="HatenaDateTime"/> オブジェクトの値を、等価な <see cref="DateTimeOffset"/> 値に変換します。
        /// </summary>
        /// <returns>現在の <see cref="HatenaDateTime"/> オブジェクトの値と等価な <see cref="DateTimeOffset"/> 値。</returns>
        /// <exception cref="InvalidOperationException">現在の <see cref="HatenaDateTime"/> オブジェクトの値は、 <see cref="DateTimeOffset"/> に変換できません。</exception>
        public DateTimeOffset ToDateTimeOffset()
        {
            try
            {
                return new DateTimeOffset(this.Year, this.Month, this.Day, this.Hour, this.Minute, this.Second, this.Offset);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        /// <summary>
        /// 現在の <see cref="HatenaDateTime"/> オブジェクトの値を、等価な ISO 8601 形式に変換します。
        /// </summary>
        /// <returns>現在の <see cref="HatenaDateTime"/> オブジェクトの値と等価な ISO 8601 形式。</returns>
        public override string ToString()
        {
            var year = this.Year.ToString("0000");
            var month = this.Month.ToString("00");
            var day = this.Day.ToString("00");
            var hour = this.Hour.ToString("00");
            var minute = this.Minute.ToString("00");
            var second = this.Second.ToString("00");
            var offset = this.Offset == TimeSpan.Zero ? "Z" : this.Offset.ToString($@"{(this.Offset < TimeSpan.Zero ? @"\-" : @"\+")}hh\:mm\:ss");

            return $"{year}-{month}-{day}T{hour}:{minute}:{second}{offset}";
        }

        /// <summary>
        /// 指定された ISO 8601 形式の日時を、等価な <see cref="HatenaDateTime"/> 値に変換します。
        /// </summary>
        /// <param name="input">変換する日時を表す ISO 8601 形式の文字列。</param>
        /// <returns><paramref name="input"/> が表す日時と等価な <see cref="HatenaDateTime"/> 値。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> が <see langword="null"/> です。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="input"/> が表す日時の要素のいずれかが、有効な範囲にありません。</exception>
        /// <exception cref="FormatException"><paramref name="input"/> が正しい ISO 8601 形式の日時ではありません。</exception>
        public static HatenaDateTime Parse(string input)
        {
            Validation.NotNull(input, nameof(input));

            var groups = _regex.Match(input).Groups;

            T GetValue<T>(Group group, Func<string, T> parse)
                => group.Success ? parse(group.Value) : throw new FormatException("Invalid format string.");

            var year = GetValue(groups["year"], int.Parse);
            var month = GetValue(groups["month"], int.Parse);
            var day = GetValue(groups["day"], int.Parse);
            var hour = GetValue(groups["hour"], int.Parse);
            var minute = GetValue(groups["minute"], int.Parse);
            var second = GetValue(groups["second"], int.Parse);
            var offset = GetValue(groups["offset"], s => s == "Z" ? TimeSpan.Zero : TimeSpan.Parse(s));

            return new HatenaDateTime(year, month, day, hour, minute, second, offset);
        }
    }
}
