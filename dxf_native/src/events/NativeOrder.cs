﻿using System;
using System.Globalization;
using com.dxfeed.api;
using com.dxfeed.native.api;

namespace com.dxfeed.native.events {
	public struct NativeOrder : IDxOrder {
		private readonly DxOrder order;
		private readonly DxString marketMaker;

		internal unsafe NativeOrder(DxOrder* order) {
			this.order = *order;
			marketMaker = DxMarshal.ReadDxString(this.order.market_maker);
		}

		public override string ToString() {
			return string.Format(CultureInfo.InvariantCulture, "Order: {{{1} {7}@{6}, Index: {0:x4}, Level: {2}, Time: {3}, ExchangeCode: '{4}', MarketMaker: '{5}'}}",
				Index, Side, Level, Time, ExchangeCode, MarketMaker, Price, Size);
		}

		#region Implementation of IDxOrder

		public long Index {
			get { return order.index; }
		}

		public Side Side {
			get { return order.side; }
		}

		public int Level {
			get { return order.level; }
		}

		public long Time {
			get { return order.time; }
		}

		public char ExchangeCode {
			get { return order.exchange_code; }
		}

		public DxString MarketMaker {
			get { return marketMaker; }
		}

		public double Price {
			get { return order.price; }
		}

		public long Size {
			get { return order.size; }
		}

		#endregion
	}
}